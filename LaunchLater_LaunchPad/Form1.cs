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

        
        private DateTime startedTime;
        private ContextMenu contextMenu = new ContextMenu();

        private System.Threading.Timer contextMenuUpdateTimer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
                        
            LLApplicationsManager.Start();
            initTrayIcon();
            monitorTimer = new System.Threading.Timer(new TimerCallback(cleanUp), null, 0, 5000);
                        
        }

        private void initTrayIcon()
        {
            LLApplicationsManager.AppStarting += new LLApplicationsManager.AppStartingEventHandler(LLApplicationsManager_AppStarting);
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Launch Later";
            trayIcon.Icon = new Icon("settings.ico");
            trayIcon.Visible = true;

            trayIcon.ShowBalloonTip(0, "LaunchLater", "Executing application schedule...", ToolTipIcon.Info);

            startedTime = DateTime.Now;
            populateContextMenu();

            trayIcon.ContextMenu = contextMenu;
            contextMenuUpdateTimer = new System.Threading.Timer(new TimerCallback(updateContextMenu), null, 1000, 1000);
            
        }

        private void populateContextMenu()
        {
            LLApplicationsManager.ApplicationTimers.ForEach(x =>
            {
                updateContextMenuItem(x);
            });
        }


        private void updateContextMenuItem(LLAppTimer x)
        {
            //look for existing menu item
            MenuItem menuItem = null;
            foreach (MenuItem m in contextMenu.MenuItems)
            {
                if (m.Text.StartsWith(x.App.Name))
                {
                    menuItem = m;
                    break;
                }
            }
            
            //get seconds remaining
            double seconds = getSecondsRemaining(x.App);
           
            //if the menu item already exists, just update it, otherwise add it
            if (menuItem != null)
            {           
                //if there are no seconds remaining, remove it from the list, otherwise update the time.
                if (seconds < 0)
                      contextMenu.MenuItems.Remove(menuItem);
                else
                      menuItem.Text = x.App.Name + " in " + seconds.ToString() + " seconds";
            }
            else
            {
                if (seconds > 0)
                {
                    contextMenu.MenuItems.Add(new MenuItem(x.App.Name + " in " + getSecondsRemaining(x.App).ToString() + " seconds"));
                }
            }
        }

        private void updateContextMenu(object stateInfo)
        {
            populateContextMenu();
        }

        private double getSecondsRemaining(LLApplication app)
        {
            TimeSpan timeSinceStarted = DateTime.Now - startedTime;
            double secondsPassed = timeSinceStarted.TotalSeconds;

            return (int)(app.DelaySeconds - secondsPassed);
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
