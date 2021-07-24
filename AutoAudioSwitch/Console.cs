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
            ParseConfig("AutoAudioSwitch.ini");

            WqlEventQuery query =
                new WqlEventQuery("__InstanceCreationEvent",
                new TimeSpan(0, 0, 1),
                "TargetInstance ISA 'Win32_Process' and TargetInstance.Name = 'EscapeFromTarkov.exe'");

            ManagementEventWatcher watcher = new ManagementEventWatcher();
            watcher.Query = query;

            watcher.EventArrived += new EventArrivedEventHandler(HandleEvent);

            watcher.Start();

            Log("Started Watcher");



        }

        private void HandleEvent(object sender, EventArrivedEventArgs e)
        {
            Log(String.Format("Process {0} has started, setting audio in 30s",
            ((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"]));

            aTimer = new System.Timers.Timer(30000);
            aTimer.Elapsed += SetAudioTimer;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
        }


        private void Console_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
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

        private void ParseConfig(string filename)
        {
            Log("Filename: " + filename);

            if (!File.Exists(filename))
            {
                Log("Could not find configuration file");
                return;
            }

            var parser = new FileIniDataParser();
            try
            {
                config = parser.ReadFile(filename);
            }
            catch (FileNotFoundException)
            {
                Log("Could not find configuration file at: " + filename);
            }
            catch (ParsingException e)
            {
                Log("Unable to parse configuration file [" + filename + "]: " + e.Message);
            }

            //foreach (SectionData section in config.Sections) {
            //Console.WriteLine(section.SectionName);

            //Iterate through all the keys in the current section
            //printing the values
            //foreach(KeyData key in section.Keys)
            //   Console.WriteLine(key.KeyName + " = " + key.Value);
            //}
        }

        private void SetAudioTimer(Object source, ElapsedEventArgs e)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            Log("Setting Tarkov Audio To Default");
            pProcess.StartInfo.FileName = @".\SoundVolumeView.exe";
            pProcess.StartInfo.Arguments = "/SetAppDefault \"DefaultRenderDevice \" 0 \"EscapeFromTarkov.exe\""; //argument
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            pProcess.Start();
            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
            Log(output);
            pProcess.WaitForExit();

            pProcess = new System.Diagnostics.Process();
            Log("Setting Tarkov Audio To Go-XLR-Game");
            pProcess.StartInfo.FileName = @".\SoundVolumeView.exe";
            pProcess.StartInfo.Arguments = "/SetAppDefault \"" + config["EscapeFromTarkov.exe"]["Device"] + "\" 0 \"EscapeFromTarkov.exe\""; //argument
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
            pProcess.Start();
            output = pProcess.StandardOutput.ReadToEnd(); //The output result
            Log(output);
            pProcess.WaitForExit();
        }

    }
}
