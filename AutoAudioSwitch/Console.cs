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
        private const string tagName = "1.1";
        private const int defaultDelay = 30;
        private static System.Timers.Timer aTimer;
        private static IniData config;
        private delegate void SafeCallDelegate(string text);

        public Console()
        {
            InitializeComponent();
        }

        private void Console_Load(object sender, EventArgs e)
        {
            Log($"AutoAudioSwitch {tagName}");

            if (ParseConfig("AutoAudioSwitch.ini") == false)
            {
                Log("INI Parsing failed, correct the above errors.");
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
            string processName = (String)((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"];
            foreach (SectionData section in config.Sections)
            {
                if ( processName == section.SectionName) {
                    int delay = defaultDelay;
                    if (section.Keys.ContainsKey("Delay"))
                    {
                        delay = Int32.Parse(section.Keys["Delay"]);
                    }
                    Log("Watched process '"+section.SectionName+"' started. Invoking timer in " + delay.ToString() + " seconds.");

                    aTimer = new System.Timers.Timer(delay * 1000);
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
                logConsole.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}: {text}\n");
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

            Log("Ini file loaded, checking...");

            foreach (SectionData section in config.Sections)
            {
                if (section.Keys.ContainsKey("Device"))
                {
                    Log(section.SectionName + ": Device set to " + section.Keys["Device"]);
                } else
                {
                    Log(section.SectionName + ": Device is not set, Device is required");
                    return false;
                }
                if (section.Keys.ContainsKey("Delay"))
                {
                    try
                    {
                        Int32.Parse(section.Keys["Delay"]);
                    }
                    catch (FormatException)
                    {
                        Log(section.SectionName + ": Device delay is not an integer");
                        return false;
                    }
                    Log(section.SectionName + ": Device delay set to " + section.Keys["Delay"] + " seconds.");
                }
                else
                {
                    Log(section.SectionName + ": Delay will be default");
                }


            }
            Log("Ini file OK");
            return true;
        }
        

        private void ChangeProcessAudio(Object source, ElapsedEventArgs e, string name)
        {
            ChangeProcessAudio(name);
        }
        private void ChangeProcessAudio(string name)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

            pProcess = new System.Diagnostics.Process();
            Log("Setting "+ name + " Audio To " + config[name]["Device"]);
            pProcess.StartInfo.FileName = @".\SoundVolumeView\SoundVolumeView.exe";
            pProcess.StartInfo.Arguments = "/SetAppDefault \"" + config[name]["Device"] + "\" all \"" + name + "\"";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            pProcess.Start();

            pProcess.WaitForExit();

            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
            if (output != "")
            {
                Log(output);
            }
        }

    }
}
