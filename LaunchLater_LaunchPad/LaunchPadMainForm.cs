using LaunchLaterOM;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LaunchLaterOM.Configuration;

namespace LaunchLater_LaunchPad
{
    public partial class LaunchPadMainForm : Form
    {
        private NotifyIcon trayIcon;
        private System.Threading.Timer monitorTimer;


        private bool paused = false;
        private ContextMenu contextMenu = new ContextMenu();

        private System.Threading.Timer contextMenuUpdateTimer;

        public LaunchPadMainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LLApplicationsManager.Start();
            initTrayIcon();

            Thread updateThread = new Thread(new ThreadStart(checkForUpdates));
            updateThread.Start();

            monitorTimer = new System.Threading.Timer(new TimerCallback(cleanUp), null, 0, 5000);
                        
        }

        private void checkForUpdates()
        {
            string txt = Application.ProductVersion.Substring(0, 3);
            double currentVersion = double.Parse(txt);
            if (Updater.UpdateExists(currentVersion))
                trayIcon.ShowBalloonTip(0, "An update is available!", @"Go to http://launchlater.codeplex.com or start LaunchLater Configuration to get the update!", ToolTipIcon.Info);

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

            MenuItem configItem = new MenuItem() { Text = "Configuration" };
            configItem.Click += new EventHandler(configItem_Click);

            MenuItem m2 = new MenuItem() { Text = "Pause All" };
            m2.Click +=new EventHandler(PauseResume_Click);
            contextMenu.MenuItems.Add(m2);
            contextMenu.MenuItems.Add(configItem);
            contextMenu.MenuItems.Add(m);
        }

        void configItem_Click(object sender, EventArgs e)
        {
            try
            {
                string pathToConfigApp = Application.ExecutablePath.Remove(Application.ExecutablePath.Length - 17, 17) + "\\LaunchLaterManager.exe";
                Process.Start(pathToConfigApp);
            }
            catch
            {
                EventLog.WriteEntry("LL_LauchPad", "Unable to launch configuration UI.", EventLogEntryType.Error);
            }

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
            try
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
                        txt = x.App.Name + " in " + TimeSpan.FromSeconds(seconds).ToString().Substring(3,5);
                        if (menuItem.Tag == "Paused") txt += " (Paused)";
                        menuItem.Text = txt;
                    }
                }
                else
                {
                    if (seconds > 0 && !x.Started)
                    {
                        MenuItem m = new MenuItem(x.App.Name + " in " + TimeSpan.FromSeconds(x.GetSecondsRemaining()).ToString().Substring(3,5));
                       
                        MenuItem subPause = new MenuItem("Pause/Resume");
                        MenuItem subExecute = new MenuItem("Execute");
                        MenuItem subCancel = new MenuItem("Cancel");

                        subPause.Click += new EventHandler(subPause_Click);
                        subExecute.Click += new EventHandler(subExecute_Click);
                        subCancel.Click += new EventHandler(subCancel_Click);

                        m.MenuItems.Add(subPause);
                        m.MenuItems.Add(subExecute);
                        m.MenuItems.Add(subCancel);

                        contextMenu.MenuItems.Add(m);
                        m.Click += new EventHandler(m_Click);

                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("LaunchPad", "There was an error creating the LaunchPad context menu. " + ex.ToString(), EventLogEntryType.Error);
            }
        }

        private MenuItem getParent(object m)
        {
            return (MenuItem)((MenuItem)m).Parent;
        }

        void subCancel_Click(object sender, EventArgs e)
        {
            string appName = getParent(sender).Text.Split(' ').First();
            LLAppTimer timer = LLApplicationsManager.ApplicationTimers.Where(x => x.App.Name == appName).First();
            LLApplicationsManager.ApplicationTimers.Remove(timer);
            timer.Pause();  //By pausing, the timer is disposed. Unless Resume() is called, this is effectively a "cancel" operation.
            contextMenu.MenuItems.Remove(getParent(sender));
        }

        void subExecute_Click(object sender, EventArgs e)
        {
            string appName = getParent(sender).Text.Split(' ').First();
            LLApplicationsManager.ApplicationTimers.Where(x => x.App.Name == appName).First().ExecutePreemptively();
            contextMenu.MenuItems.Remove(getParent(sender));
        }

        void subPause_Click(object sender, EventArgs e)
        {
            string appName = getParent(sender).Text.Split(' ').First();
            if (getParent(sender).Tag == "Paused")
            {
                LLApplicationsManager.ApplicationTimers.Where(x => x.App.Name == appName).First().Resume();
                //((MenuItem)((MenuItem)sender).Parent).Text.Replace(" (Paused)", "");
                ((MenuItem)((MenuItem)sender).Parent).Tag = "";
            }
            else
            {
                LLApplicationsManager.ApplicationTimers.Where(x => x.App.Name == appName).First().Pause();
                //((MenuItem)((MenuItem)sender).Parent).Text += " (Paused)";
                getParent(sender).Tag = "Paused";
            }
            
        }

        void Exit_Click(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            Application.Exit();
        }

        void PauseResume_Click(object sender, EventArgs e)
        {
            if (paused)
            {
                LLApplicationsManager.ApplicationTimers.Where(x => x.Started == false).ToList().ForEach(x => x.Resume());

                foreach (MenuItem item in contextMenu.MenuItems)
                {
                    item.Tag = "";
                }
                paused = false;
                ((MenuItem)sender).Text = "Pause All";
            }
            else
            {
                LLApplicationsManager.ApplicationTimers.Where(x => x.Started == false).ToList().ForEach(x => x.Pause());
                
                foreach (MenuItem item in contextMenu.MenuItems)
                {
                    item.Tag = "Paused";
                }
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
            try
            {
                EventLog.WriteEntry("LaunchPad", "Launching application: " + e.Name);

                trayIcon.ShowBalloonTip(0, "LaunchLater", "Executing " + e.Name, ToolTipIcon.Info);
            }
            catch
            {
                //swallow
            }
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
