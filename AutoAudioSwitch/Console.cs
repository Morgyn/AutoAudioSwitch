using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoAudioSwitch
{
    public partial class Console : Form
    {
        public Console()
        {
            InitializeComponent();
        }

        private void Console_Load(object sender, EventArgs e)
        {
            Log("test");
        }

        private void Console_Resize(object sender, EventArgs e)
        {
            Log("test2");
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

            logConsole.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}: " + text + "\n");
            logConsole.ScrollToCaret();
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
    }
}
