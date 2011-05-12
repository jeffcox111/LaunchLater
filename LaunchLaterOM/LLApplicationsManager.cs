using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace LaunchLaterOM
{
    public static class LLApplicationsManager
    {
        public static List<LLAppTimer> ApplicationTimers { get; set; }

        public delegate void AppStartingEventHandler(object sender, AppStartingEventArgs e);

        public static event AppStartingEventHandler AppStarting;

        public static void Start()
        {
            EventLog.WriteEntry("LaunchPad", "Staring LaunchPad...");

            ApplicationTimers = new List<LLAppTimer>();

            LLConfiguration config = getConfiguration();

            var apps = config.DefaultProfile.Applications.Where(x => x.Enabled == true).ToList();
            apps = (from a in apps
                   orderby a.DelaySeconds ascending
                   select a).ToList();
            apps.ForEach(x => ApplicationTimers.Add(new LLAppTimer(x)));
            ApplicationTimers.ForEach(x => x.AppStarting +=new LLAppTimer.AppStartingEventHandler(x_AppStarting));
        }

        private static LLConfiguration getConfiguration()
        {
            if (File.Exists(Environment.SpecialFolder.ApplicationData.ToString() + @"\LaunchLater\LaunchLaterApps.config"))
                return new LLConfiguration(true);
            else
                return new LLConfiguration(true);
        }

        static void x_AppStarting(object sender, AppStartingEventArgs e)
        {
            try
            {
                AppStarting(null, new AppStartingEventArgs { Name = e.Name });
            }
            catch (Exception ex)
            {
                //swallow
            }
        }

        public static void Stop()
        {
            EventLog.WriteEntry("LaunchPad", "Stopping LaunchPad...");
            ApplicationTimers.Clear();
        }

        public static int NumberOfAppsRemaining()
        {
            var pendingAppsCount = (from a in ApplicationTimers
                                    where a.Started == false
                                    select a).Count();

            return pendingAppsCount;
        }

       

    }

public class AppStartingEventArgs : EventArgs 
{
    public string Name {get;set;}
}
}
