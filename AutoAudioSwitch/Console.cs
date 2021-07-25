using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;
using System.IO;
using IniParser;
using IniParser.Model;
using IniParser.Exceptions;

namespace AutoAudioSwitch
{
    public partial class Console : Form
    {
        private static System.Timers.Timer aTimer;
        private static IniData config;
        private delegate void SafeCallDelegate(string text);

        public Console()
        {
            InitializeComponent();
        }

        private void Console_Load(object sender, EventArgs e)
        {
            if (ParseConfig("AutoAudioSwitch.ini") == false)
            {
                return;
            }

            WqlEventQuery query =
                new WqlEventQuery("__InstanceCreationEvent",
                new TimeSpan(0, 0, 1),
                "TargetInstance ISA 'Win32_Process'");

            ManagementEventWatcher watcher = new ManagementEventWatcher();
            watcher.Query = query;

            watcher.EventArrived += new EventArrivedEventHandler(HandleEvent);

            watcher.Start();

            Log("Started Watcher");

            Log("Checking if processes are already running:");

            //iterate over processes
            foreach (SectionData section in config.Sections)
            {
                if (CheckProcess(section.SectionName))
                {
                    Log(section.SectionName +" is running, changing audio.");
                    ChangeProcessAudio(section.SectionName);
                }
                else
                {
                    Log(section.SectionName + " is not running");
                }
            }
         }

        public bool CheckProcess(string name)
        {
            try
            {
                string Query = "SELECT Name FROM Win32_Process WHERE Name = '" + name + "'";
                ManagementObjectSearcher mos = new ManagementObjectSearcher(Query);

                if (mos.Get().Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            {
                //ex.HandleException();
            }
            return false;
        }

        private void HandleEvent(object sender, EventArrivedEventArgs e)
        {
            foreach (SectionData section in config.Sections)
            {
                if ((String)((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"] == section.SectionName) {
                    Log("Watched process '"+section.SectionName+"' started. Invoking timer in 30 seconds.");

                    aTimer = new System.Timers.Timer(30000);
                    aTimer.Elapsed += (senderr, ee) => ChangeProcessAudio(senderr, ee, section.SectionName); ;
                    aTimer.AutoReset = false;
                    aTimer.Enabled = true;
                }
            }

   //         Log(String.Format("Process {0} has started, setting audio in 30s",
   //          ((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"]));


        }


        private void Console_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        public void Log(string text)
        {
            if (IsDisposed)
            {
                return;
            }

            if (logConsole.InvokeRequired)
            {
                var d = new SafeCallDelegate(Log);
                logConsole.Invoke(d, new object[] { text });
            }
            else
            {
                logConsole.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}: " + text + "\n");
                logConsole.ScrollToCaret();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void scToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private bool ParseConfig(string filename)
        {
            Log("Filename: " + filename);

            if (!File.Exists(filename))
            {
                Log("Could not find configuration file");
                return false;
            }

            var parser = new FileIniDataParser();
            try
            {
                config = parser.ReadFile(filename);
            }
            catch (FileNotFoundException)
            {
                Log("Could not find configuration file at: " + filename);
                return false;
            }
            catch (ParsingException e)
            {
                Log("Unable to parse configuration file [" + filename + "]: " + e.Message);
                return false;
            }

            Log("Ini file loaded");
            return true;
            //foreach (SectionData section in config.Sections) {
            //Console.WriteLine(section.SectionName);

            //Iterate through all the keys in the current section
            //printing the values
            //foreach(KeyData key in section.Keys)
            //   Console.WriteLine(key.KeyName + " = " + key.Value);
            //}
        }

        private void ChangeProcessAudio(Object source, ElapsedEventArgs e, string name)
        {
            ChangeProcessAudio(name);
        }
        private void ChangeProcessAudio(string name)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            Log("Setting Tarkov Audio To Default");
            pProcess.StartInfo.FileName = @".\SoundVolumeView.exe";
            pProcess.StartInfo.Arguments = "/SetAppDefault \"DefaultRenderDevice \" 0 \""+name+"\"";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            pProcess.Start();
            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
            if (output != "")
            {
                Log(output);
            }
            pProcess.WaitForExit();

            pProcess = new System.Diagnostics.Process();
            Log("Setting Tarkov Audio To Go-XLR-Game");
            pProcess.StartInfo.FileName = @".\SoundVolumeView.exe";
            pProcess.StartInfo.Arguments = "/SetAppDefault \"" + config["EscapeFromTarkov.exe"]["Device"] + "\" 0 \"" + name + "\"";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            pProcess.Start();
            output = pProcess.StandardOutput.ReadToEnd(); //The output result
            if (output != "")
            {
                Log(output);
            }
            pProcess.WaitForExit();
        }

    }
}
