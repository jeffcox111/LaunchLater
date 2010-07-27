using System;
using System.Diagnostics;
using System.Threading;

namespace LaunchLaterOM
{
    public class LLAppTimer
    {
        public delegate void AppStartingEventHandler(object sender, AppStartingEventArgs e);

        public event AppStartingEventHandler AppStarting;

        public LLApplication App { get; set; }
        public Timer AppTimer { get; private set; }
        public bool Started { get; private set; }

        private bool paused = false;
        private int resumeSeconds = 0;

        private DateTime startedTime = DateTime.Now;

        public LLAppTimer() { }

        public LLAppTimer(LLApplication app)
        {
            if (app == null) 
                return;

            Started = false;
            App = app;
            AppTimer = new Timer(new TimerCallback(callback), null, App.DelaySeconds * 1000, App.DelaySeconds * 1000);
            startedTime = DateTime.Now;

        }

        public int GetSecondsRemaining()
        {
            if (paused)
                return resumeSeconds;
        
            TimeSpan timeSinceStarted = DateTime.Now - startedTime;
            double secondsPassed = timeSinceStarted.TotalSeconds;

            return (int)(this.App.DelaySeconds - secondsPassed);
        }

        public void Pause()
        {
            try
            {
                resumeSeconds = this.GetSecondsRemaining();
                paused = true; 

                AppTimer.Dispose();
                AppTimer = null;
            }
            catch 
            {
            }

        }

        public void Resume()
        {
            if (paused)
            {
                App.DelaySeconds = resumeSeconds;
                AppTimer = new Timer(new TimerCallback(callback), null, App.DelaySeconds * 1000, App.DelaySeconds * 1000);
                startedTime = DateTime.Now;
                paused = false;
                
            }
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

        public void ExecutePreemptively()
        {
            AppTimer.Dispose();
            callback(true);

        }

    }
}
