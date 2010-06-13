using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace LaunchLaterOM
{
    public static class LLApplicationsManager
    {
        private static List<LLAppTimer> ApplicationTimers { get; set; }

        public delegate void AppStartingEventHandler(object sender, AppStartingEventArgs e);

        public static event AppStartingEventHandler AppStarting;

        public static void Start()
        {
            EventLog.WriteEntry("LaunchPad", "Staring LaunchPad...");

            ApplicationTimers = new List<LLAppTimer>();

            LLConfiguration config = new LLConfiguration("LaunchLaterApps.config");

            var apps = config.DefaultProfile.Applications;
            apps.ForEach(x => ApplicationTimers.Add(new LLAppTimer(x)));
            ApplicationTimers.ForEach(x => x.AppStarting +=new LLAppTimer.AppStartingEventHandler(x_AppStarting));
        }

        static void x_AppStarting(object sender, AppStartingEventArgs e)
        {
            AppStarting(null, new AppStartingEventArgs { Name = e.Name });

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
