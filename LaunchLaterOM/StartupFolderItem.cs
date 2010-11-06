using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchLaterOM
{
    public class StartupFolderItem : StartupItem
    {
        public string FileName { get; set; }
        public string ShortcutFullPath { get; set; }

        public override string Name
        {
            get { return FileName.Remove(FileName.Length - 4, 4); }
        }

        public StartupFolderItem(string fileName, string args, string fullPath, string shortcutFullPath)
            : base(fullPath, args)
        {
            FileName = fileName;
            ShortcutFullPath = shortcutFullPath;
            IsChecked = ShouldBeChecked();
        }
    }
}
