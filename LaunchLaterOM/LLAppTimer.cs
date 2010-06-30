using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace LaunchLaterOM
{
    public class LLAppTimer
    {
        public delegate void AppStartingEventHandler(object sender, AppStartingEventArgs e);

        public event AppStartingEventHandler AppStarting;

        public LLApplication App { get; set; }
        public Timer AppTimer { get; private set; }
        public bool Started { get; private set; }

        public LLAppTimer() { }

        public LLAppTimer(LLApplication app)
        {
            if (app == null) 
                return;

            Started = false;
            App = app;
            AppTimer = new Timer(new TimerCallback(callback), null, App.DelaySeconds * 1000, App.DelaySeconds * 1000);
            
        }
       
        private void callback(object stateInfo)
        {
            if (LLUtilities.LLIsTryingToRunItself(App.FullPath))
            {
                Started = true;
                return;
            }

            AppStarting(this, new AppStartingEventArgs { Name = App.Name });

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = App.FullPath;
                p.StartInfo.Arguments = App.Arguments;

                p.Start();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("LaunchLater", "Error executing app... " + ex.ToString());
                throw new LLApplicationNotFoundException();
            }
            finally
            {
                Started = true;
                AppTimer.Dispose();
            }
        }

    }
}
