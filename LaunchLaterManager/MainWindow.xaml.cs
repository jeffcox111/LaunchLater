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
using LaunchLaterManager.Views;

namespace LaunchLaterManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static LLConfiguration config;

        private AppsListViewModel appsListVM;

        private static List<LLApplication> _startupItemsToBeDeleted = new List<LLApplication>();
        private static List<LLApplication> _startupItemsToBeRestored = new List<LLApplication>();

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
                        select new AppViewModel { App = a }).ToList();

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
                delegate()
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

            AppSortingStyle style;

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

        void AppsListBox_OnAppDeleted(object sender, AppDeletedEventArgs e)
        {
            var app = ((AppView)sender).App;

            var vm = (from v in appsListVM.Applications
                      where v.App == app
                      select v).FirstOrDefault();


            if (vm == null)
            {
                app = config.DefaultProfile.Applications.Last();
                vm = appsListVM.Applications.Last();
            }

            if (e.ShouldRestoreToStartupItems)
            {
                // restore the item before deleting it
                //if (app.IsImported)
                    _startupItemsToBeRestored.Add(app);                    
            }

            appsListVM.Applications.Remove(vm);
            config.DefaultProfile.Applications.Remove(app);            

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
                    foreach (var app in _startupItemsToBeDeleted)
                        StartupItem.Delete(app);
                    foreach (var app in _startupItemsToBeRestored)
                        StartupItem.Restore(app);

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
            LLApplication newApp = new LLApplication() { Arguments = "", DelaySeconds = 0, FullPath = "", Name = "", Enabled = true };
            config.DefaultProfile.Applications.Add(newApp);
            AppViewModel appVM = new AppViewModel() { App = newApp };
            appsListVM.Applications.Add(appVM);
            config.IsDirty = true;

        }

        private void ImportStartupItemsButton_Click(object sender, RoutedEventArgs e)
        {
            var startupItemsWindow = new StartupItemsWindow();
            startupItemsWindow.ApplicationCreated += (newApp) => 
            {
                if (!config.DefaultProfile.Applications.Where(x => x.FullPath == newApp.FullPath && x.Arguments == newApp.Arguments && x.DelaySeconds == newApp.DelaySeconds).Any())
                {
                    config.DefaultProfile.Applications.Add(newApp);
                    var appVM = new AppViewModel() { App = newApp };
                    appsListVM.Applications.Add(appVM);
                    config.IsDirty = true;
                    _startupItemsToBeDeleted.Add(newApp);
                }
            };
            startupItemsWindow.Show();
        }        

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://launchlater.codeplex.com"));
        }

    }
}
