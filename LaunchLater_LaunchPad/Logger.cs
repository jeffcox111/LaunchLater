using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LaunchLater_LaunchPad
{
    public static class Logger
    {
        public static void LogInfo(string message)
        {
            try
            {
                EventLog.WriteEntry("LL_LauchPad", message, EventLogEntryType.Information);
            }
            catch { }
        }

        public static void LogError(string message)
        {
            try
            {
                EventLog.WriteEntry("LL_LauchPad", message, EventLogEntryType.Error);
            }
            catch { }
        }
    }
}
