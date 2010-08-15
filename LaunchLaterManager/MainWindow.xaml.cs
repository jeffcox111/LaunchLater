using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaunchLaterManager.ViewModels;
using System.Collections.ObjectModel;
using LaunchLaterOM;
using Microsoft.Win32;
using System.IO;

namespace LaunchLaterManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static LLConfiguration config;



        LLApplicationsListViewModel llvm;
        public MainWindow()
        {
            InitializeComponent();

        }


        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            llvm = new LLApplicationsListViewModel();

            config = new LLConfiguration("LaunchLaterApps.config");

            var apps = (from a in config.DefaultProfile.Applications
                        select new LLApplicationViewModel { App = a }).ToList();

            ObservableCollection<LLApplicationViewModel> appViewModels = new ObservableCollection<LLApplicationViewModel>();
            apps.ForEach(x => appViewModels.Add(x));

            llvm.Applications = appViewModels;

            AppsListBox.DataContext = llvm;
            AppsListBox.OnChangeHasBeenMade += new AppListBox.ChangeHasBeenMadeHandler(AppsListBox_OnChangeHasBeenMade);
            AppsListBox.OnAppDeleted += new AppListBox.DeleteAppHandler(AppsListBox_OnAppDeleted);

        }

        void AppsListBox_OnAppDeleted(object sender, EventArgs e)
        {
            var app = ((AppView)sender).App;

            var vm = (from v in llvm.Applications
                      where v.App == app
                      select v).FirstOrDefault();
            if (vm == null)
            {
                llvm.Applications.Remove(llvm.Applications.Last());
                config.DefaultProfile.Applications.Remove(config.DefaultProfile.Applications.Last());
            }
            else
            {
                llvm.Applications.Remove(vm);
                config.DefaultProfile.Applications.Remove(app);
            }

            config.IsDirty = true;
        }

        void AppsListBox_OnChangeHasBeenMade(object sender, EventArgs e)
        {
            config.IsDirty = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!config.IsDirty)
                Application.Current.Shutdown();
            else
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save changes?", "LaunchLater", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    config.WriteFreshConfigurationFile("LaunchLaterApps.config");
                    Application.Current.Shutdown();
                }
                else if (result == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LLApplication newApp = new LLApplication() { Arguments = "", DelaySeconds = 0, FullPath = "", Name = "" };
            config.DefaultProfile.Applications.Add(newApp);
            LLApplicationViewModel appVM = new LLApplicationViewModel() { App = newApp };
            llvm.Applications.Add(appVM);

        }

        private void ImportRegistryStartupItems()
        {
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

                        var newApp = new LLApplication { Arguments = argsString, DelaySeconds = 0, FullPath = filePath, Name = reg.Key };
                        if (!config.DefaultProfile.Applications.Where(x => x.FullPath == newApp.FullPath && x.Arguments == newApp.Arguments && x.DelaySeconds == newApp.DelaySeconds).Any())
                        {
                            config.DefaultProfile.Applications.Add(newApp);
                            var appVM = new LLApplicationViewModel() { App = newApp };
                            llvm.Applications.Add(appVM);
                            DeleteRegValue(regKey, runKey, reg.Key);
                            config.IsDirty = true;
                        }                        
                    }
                }
            }
        }

        private void DeleteRegValue(RegistryKey regKey, string registryPath, string valueKey)
        {
            var registeryKey = regKey.OpenSubKey(registryPath, true);
            if (registeryKey != null)
            {
                registeryKey.DeleteValue(valueKey, false);
            }
        }

        private void ImportStartupFolderItems()
        {
            // check startup folder for current user
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(startupFolder))
            {
                var files = LLUtilities.GetFiles(startupFolder, "*.lnk|*.exe");
                foreach (var file in files)
                {
                    var filePath = file;
                    if (file.Contains(".lnk"))
                        filePath = LLUtilities.ResolveShortcut(file);

                    // make sure we skip LL if it is somehow in the registry
                    if (LLUtilities.LLIsTryingToRunItself(filePath))
                        continue;

                    // parse the file name in case the shortcut was bad
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                    {
                        var newApp = new LLApplication { Arguments = string.Empty, DelaySeconds = 0, FullPath = fileInfo.FullName, Name = fileInfo.Name.Remove(fileInfo.Name.Length - 4, 4) };
                        if (!config.DefaultProfile.Applications.Where(x => x.FullPath == newApp.FullPath && x.Arguments == newApp.Arguments && x.DelaySeconds == newApp.DelaySeconds).Any())
                        {
                            config.DefaultProfile.Applications.Add(newApp);
                            var appVM = new LLApplicationViewModel() { App = newApp };
                            llvm.Applications.Add(appVM);
                            File.Delete(file);
                            config.IsDirty = true;
                        }
                    }
                }
            }
        }

        private void ImportStartupItemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Would you like to load the existing startup items and allow LaunchLater to manage them?", "Load Existing Apps", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ImportRegistryStartupItems();
                ImportStartupFolderItems();
                config.WriteFreshConfigurationFile("LaunchLaterApps.config");
                config.IsDirty = false;
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetRegistryItemsForKey(RegistryKey regKey, string registryPath)
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
