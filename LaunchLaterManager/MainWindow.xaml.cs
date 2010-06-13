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
            apps.ForEach(x=> appViewModels.Add(x));

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
                      select v).First();

            llvm.Applications.Remove(vm);
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
            LLApplication newApp = new LLApplication() { Arguments="", DelaySeconds=0, FullPath="", Name="" };
            config.DefaultProfile.Applications.Add(newApp);
            LLApplicationViewModel appVM = new LLApplicationViewModel() { App = newApp };
        	llvm.Applications.Add(appVM);

        }

    }
}
