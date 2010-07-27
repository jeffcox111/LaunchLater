using LaunchLaterOM;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LaunchLater_LaunchPad
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private System.Threading.Timer monitorTimer;


        private bool paused = false;
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

            //startedTime = DateTime.Now;
            populateContextMenu();

            createPauseAndExitItems();

            trayIcon.ContextMenu = contextMenu;
            contextMenuUpdateTimer = new System.Threading.Timer(new TimerCallback(updateContextMenu), null, 1000, 1000);
            
        }

        private void createPauseAndExitItems()
        {
            MenuItem m = new MenuItem() { Text = "Exit" };
            m.Click += new EventHandler(Exit_Click);
            

            MenuItem m2 = new MenuItem() { Text = "Pause All" };
            m2.Click +=new EventHandler(PauseResume_Click);
            contextMenu.MenuItems.Add(m2);
            contextMenu.MenuItems.Add(m);
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
            double seconds = x.GetSecondsRemaining();
           
            //if the menu item already exists, just update it, otherwise add it
            if (menuItem != null)
            {           
                //if there are no seconds remaining, remove it from the list, otherwise update the time.
                if (seconds < 0)
                    contextMenu.MenuItems.Remove(menuItem);
                else
                {
                    string txt = "";
                    txt = x.App.Name + " in " + seconds.ToString() + " seconds";
                    if (paused) txt += " (Paused)";
                    menuItem.Text = txt;
                }
            }
            else
            {
                if (seconds > 0 && !x.Started)
                {
                    MenuItem m = new MenuItem(x.App.Name + " in " + x.GetSecondsRemaining().ToString() + " seconds");
                    contextMenu.MenuItems.Add(m);
                    m.Click += new EventHandler(m_Click);

                }
            }
        }

        void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void PauseResume_Click(object sender, EventArgs e)
        {
            if (paused)
            {
                LLApplicationsManager.ApplicationTimers.Where(x => x.Started == false).ToList().ForEach(x => x.Resume());
                paused = false;
                ((MenuItem)sender).Text = "Pause All";
            }
            else
            {
                LLApplicationsManager.ApplicationTimers.Where(x => x.Started == false).ToList().ForEach(x => x.Pause());
                paused = true;
                ((MenuItem)sender).Text = "Resume All";
            }

        }

        void m_Click(object sender, EventArgs e)
        {
            
            string appName = ((MenuItem)sender).Text.Split(' ').First();
            LLApplicationsManager.ApplicationTimers.Where(x => x.App.Name == appName).First().ExecutePreemptively();
            contextMenu.MenuItems.Remove((MenuItem)sender);
        }

        private void updateContextMenu(object stateInfo)
        {
            populateContextMenu();
            
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
