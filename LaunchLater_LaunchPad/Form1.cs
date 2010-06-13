using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LaunchLaterOM;
using System.Threading;
using System.Diagnostics;

namespace LaunchLater_LaunchPad
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private System.Threading.Timer monitorTimer;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LLApplicationsManager.AppStarting += new LLApplicationsManager.AppStartingEventHandler(LLApplicationsManager_AppStarting);
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Launch Later";
            trayIcon.Icon = new Icon("settings.ico");
            trayIcon.Visible = true;
            LLApplicationsManager.Start();

            monitorTimer = new System.Threading.Timer(new TimerCallback(cleanUp), null, 0, 5000);

            trayIcon.ShowBalloonTip(0, "LaunchLater", "Executing application schedule...", ToolTipIcon.Info);
            
        }

        void LLApplicationsManager_AppStarting(object sender, AppStartingEventArgs e)
        {
            EventLog.WriteEntry("LaunchPad", "Launching application: " + e.Name);
            
            trayIcon.ShowBalloonTip(0, "LaunchLater", "Executing " + e.Name, ToolTipIcon.Info);
        }

        private void cleanUp(object stateInfo)
        {
            if (LLApplicationsManager.NumberOfAppsRemaining() == 0)
            {
                System.Diagnostics.EventLog.WriteEntry("LaunchPad", "Finished launching all configured applications.");
                trayIcon.Dispose();
                Application.Exit();
            }
        }

       


    }
}
