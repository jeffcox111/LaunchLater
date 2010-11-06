using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchLaterOM
{
    public class StartupFolderInformation
    {
        public string ShortcutFullPath { get; set; }
        public StartupFolderInformation(string shortcutFullPath)
        {
            this.ShortcutFullPath = shortcutFullPath;
        }
    }
}
