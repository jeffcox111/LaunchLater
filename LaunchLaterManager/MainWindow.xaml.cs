using LaunchLaterManager.ViewModels;
using LaunchLaterOM;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using LaunchLaterOM.Configuration;
using System.Threading;
using System.Diagnostics;

namespace LaunchLaterManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static LLConfiguration config;

        private AppsListViewModel appsListVM;

        public MainWindow()
        {
            InitializeComponent();

        }


        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            InitLaunchLaterUI();

        }

        private void InitLaunchLaterUI()
        {
            InitSortingOptions();

            appsListVM = new AppsListViewModel();

            config = getConfiguration();
          
            var apps = (from a in config.DefaultProfile.Applications
                        select new AppViewModel{ App = a }).ToList();

            ObservableCollection<AppViewModel> appViewModels = new ObservableCollection<AppViewModel>();

            apps.ForEach(x => appViewModels.Add(x));
            appsListVM.Applications = appViewModels;
            appsListVM.SortApps(AppSortingStyle.Name);

            AppsListBox.DataContext = appsListVM;
            AppsListBox.OnChangeHasBeenMade += new AppsListView.ChangeHasBeenMadeHandler(AppsListBox_OnChangeHasBeenMade);
            AppsListBox.OnAppDeleted += new AppsListView.DeleteAppHandler(AppsListBox_OnAppDeleted);

            checkForUpdatesAsynchronously();

            
        }

        private static LLConfiguration getConfiguration()
        {
                return new LLConfiguration(true);
        }


        private void checkForUpdatesAsynchronously()
        {
            Thread updaterThread = new Thread(new ThreadStart(
                delegate ()
                {
                    cmdUpdate.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                                   {
                                       checkForUpdates();
                                   }));
                }));

            updaterThread.Start();
        }

        private void checkForUpdates()
        {
            string txt = System.Windows.Forms.Application.ProductVersion.Substring(0, 3);
            double currentVersion = double.Parse(txt);
            if (Updater.UpdateExists(currentVersion))
                cmdUpdate.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void InitSortingOptions()
        {
            cmbSorting.Items.Add("Sort by Name");
            cmbSorting.Items.Add("Sort by Delay");
            cmbSorting.Items.Add("Sort by Enabled");
            cmbSorting.SelectedItem = "Sort by Name";

            cmbSorting.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cmbSorting_SelectionChanged);
        }

        void cmbSorting_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            AppSortingStyle style ;

            switch (cmbSorting.SelectedItem.ToString())
            {
                case "Sort by Delay": style = AppSortingStyle.Timeline; break;
                case "Sort by Enabled": style = AppSortingStyle.Enabled; break;
                default: style = AppSortingStyle.Name; break;
            }

            AppsListViewModel vm = (AppsListViewModel)AppsListBox.DataContext;
            vm.SortApps(style);
            AppsListBox.DataContext = null;
            AppsListBox.DataContext = vm;


        }

        void AppsListBox_OnAppDeleted(object sender, EventArgs e)
        {
            var app = ((AppView)sender).App;

            var vm = (from v in appsListVM.Applications
                      where v.App == app
                      select v).FirstOrDefault();
            if (vm == null)
            {
                appsListVM.Applications.Remove(appsListVM.Applications.Last());
                config.DefaultProfile.Applications.Remove(config.DefaultProfile.Applications.Last());
            }
            else
            {
                appsListVM.Applications.Remove(vm);
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
                    config.WriteFreshConfigurationFile();
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
            LLApplication newApp = new LLApplication() { Arguments = "", DelaySeconds = 0, FullPath = "", Name = "", Enabled = true};
            config.DefaultProfile.Applications.Add(newApp);
            AppViewModel appVM = new AppViewModel() { App = newApp };
            appsListVM.Applications.Add(appVM);
            config.IsDirty = true;

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
                            var appVM = new AppViewModel() { App = newApp };
                            appsListVM.Applications.Add(appVM);
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
                            var appVM = new AppViewModel() { App = newApp };
                            appsListVM.Applications.Add(appVM);
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
                config.WriteFreshConfigurationFile();
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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://launchlater.codeplex.com"));
        }

    }
}
