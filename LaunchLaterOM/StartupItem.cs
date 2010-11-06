using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace LaunchLaterOM
{
    public abstract class StartupItem
    {
        public abstract string Name { get; }
        public string FullPath { get; set; }
        public string Arguments { get; set; }
        public bool IsChecked { get; set; }

        public StartupItem(string fullPath, string arguments)
        {
            FullPath = fullPath;
            Arguments = arguments;
        }

        protected bool ShouldBeChecked()
        {
            var exemptNames = new string[] { "Apple_KbdMgr", "MSSE" };

            if (exemptNames.Contains(Name))
                return false;

            return true;
        }

        public static void Restore(LLApplication app)
        {
            if (app.IsImported)
            {
                if (app.RegistryInfo != null)
                {
                    var regKeys = new List<RegistryKey>() {
                            RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default),
                            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
                    };
                    foreach (var regKey in regKeys)
                    {
                        if (regKey.Name == app.RegistryInfo.RegistryName)
                        {
                            var registeryKey = regKey.OpenSubKey(app.RegistryInfo.RegistryLocation, true);
                            if (registeryKey != null)
                                registeryKey.SetValue(app.RegistryInfo.RegistryKey, app.RegistryInfo.RegistryValue);
                        }
                    }
                }
                else if (app.FolderInfo != null)
                {
                    LLUtilities.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup), app.FullPath, app.Arguments, app.Name);
                }
            }
        }

        public static void Delete(LLApplication app)
        {
            // delete the item from it's location
            if (app.RegistryInfo != null)
            {
                var regKeys = new List<RegistryKey>() {
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32),
                        RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64),
                        RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32)
                };
                foreach (var regKey in regKeys)
                {
                    var registeryKey = regKey.OpenSubKey(app.RegistryInfo.RegistryLocation, true);
                    if (registeryKey != null)
                        if (registeryKey.GetValueNames().Contains(app.RegistryInfo.RegistryKey))
                            registeryKey.DeleteValue(app.RegistryInfo.RegistryKey, false);
                }                
            }
            else if (app.FolderInfo != null)
            {
                File.Delete(app.FolderInfo.ShortcutFullPath);
            }
        }

        public LLApplication CreateApp()
        {
            LLApplication newApp = null;
            if (this is StartupRegistryItem)
            {
                var registryItem = this as StartupRegistryItem;
                newApp = new LLApplication { Arguments = registryItem.Arguments, DelaySeconds = 0, FullPath = registryItem.FullPath, Name = registryItem.Name, Enabled = true };
                newApp.RegistryInfo = new StartupRegistryInformation(registryItem.RegistryName, registryItem.RegistryLocation, registryItem.RegistryKey, registryItem.RegistryValue);

            }
            else if (this is StartupFolderItem)
            {
                var folderItem = this as StartupFolderItem;
                newApp = new LLApplication { Arguments = folderItem.Arguments, DelaySeconds = 0, FullPath = folderItem.FullPath, Name = folderItem.Name, Enabled = true };
                newApp.FolderInfo = new StartupFolderInformation(folderItem.ShortcutFullPath);
            }
            return newApp;
        }

        public static IEnumerable<StartupItem> GetStartupItems()
        {
            var startupItems = new List<StartupItem>();
            startupItems.AddRange(PopulateFolderStartupItems());
            startupItems.AddRange(PopulateRegistryStartupItems());
            return startupItems;
        }

        private static IList<StartupItem> PopulateFolderStartupItems()
        {
            var startupItems = new List<StartupItem>();
            // check startup folder for current user
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(startupFolder))
            {
                // only look for shortcuts, we don't want to deal with actual executables in the startup directory 
                // we would have to do some weird copy to LL folder and execute there
                // we would probably have permission and locking issues as well
                var files = LLUtilities.GetFiles(startupFolder, "*.lnk");
                foreach (var file in files)
                {
                    var shortCutFile = new FileInfo(file);
                    var filePath = file;
                    var args = string.Empty;
                    if (file.Contains(".lnk"))
                    {
                        filePath = LLUtilities.ResolveShortcutPath(file);
                        args = LLUtilities.ResolveShortcutArguments(file);
                    }

                    // make sure we skip LL if it is somehow in the registry
                    if (LLUtilities.LLIsTryingToRunItself(filePath))
                        continue;

                    // parse the file name in case the shortcut was bad
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                        startupItems.Add(new StartupFolderItem(shortCutFile.Name, args, fileInfo.FullName, shortCutFile.FullName));
                }
            }
            return startupItems;
        }

        private static IList<StartupItem> PopulateRegistryStartupItems()
        {
            var startupItems = new List<StartupItem>();

            var regKeys = new List<RegistryKey>() {
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32),
                        RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64),
                        RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32)
                };

            // check registry
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            foreach (var regKey in regKeys)
            {
                if (regKey != null)
                {
                    foreach (var reg in GetRegistryItemsForKey(regKey, runKey))
                    {
                        // make sure we skip LL if it is somehow in the registry
                        if (LLUtilities.LLIsTryingToRunItself(reg.Value))
                            continue;

                        var filePath = reg.Value;
                        var argsString = string.Empty;
                        if (!File.Exists(filePath))
                        {
                            // we need to attempt to break out the path from the command arguments
                            var inQuotes = false;
                            var pathValues = reg.Value.Split(c =>
                            {
                                if (c == '\"')
                                    inQuotes = !inQuotes;

                                return !inQuotes && c == ' ';
                            })
                            .Select(arg => arg.Trim().Replace("\"", ""))
                            .Where(arg => !string.IsNullOrEmpty(arg)).ToList();
                            filePath = pathValues.FirstOrDefault();

                            // either we have a parsing error or the file doesn't exist, just skip it and move along
                            if (filePath == null || !File.Exists(filePath))
                                continue;

                            foreach (var args in pathValues)
                            {
                                if (args != filePath)
                                {
                                    if (argsString != string.Empty)
                                        argsString += " ";
                                    argsString += args;
                                }
                            }
                        }

                        startupItems.Add(new StartupRegistryItem(filePath, reg.Key, reg.Value, regKey.Name, runKey, argsString));
                    }
                }
            }

            return startupItems;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetRegistryItemsForKey(RegistryKey regKey, string registryPath)
        {
            var registeryKey = regKey.OpenSubKey(registryPath);
            if (registeryKey != null)
            {
                foreach (var registryKey in registeryKey.GetValueNames())
                {
                    yield return new KeyValuePair<string, string>(registryKey, registeryKey.GetValue(registryKey) as string);
                }
            }
        }
    }
}
