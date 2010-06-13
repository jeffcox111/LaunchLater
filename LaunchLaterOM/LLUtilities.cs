using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchLaterOM
{
    public static class LLUtilities
    {
        public static bool LLIsTryingToRunItself(string fullPath)
        {
            return fullPath.EndsWith("LL_LaunchPad.exe");
        }

        public static bool LLIsTryingToRunAntivirus(string fullPath)
        {
            return false;
        }

        public static string GetConfigPath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location.Remove(
                System.Reflection.Assembly.GetExecutingAssembly().Location.Length - 17, 17);

        }
    }
}
