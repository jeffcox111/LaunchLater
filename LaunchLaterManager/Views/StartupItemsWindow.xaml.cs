using LaunchLaterManager.ViewModels;
using LaunchLaterOM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LaunchLaterManager.Views
{
    /// <summary>
    /// Interaction logic for StartupItemsWindows.xaml
    /// </summary>
    public partial class StartupItemsWindow : Window
    {
        /// <summary>
        /// This is an event that will fire once we have created an LLApplication. If the caller returns False, the item will not be deleted from it's original location.
        /// </summary>
        public Action<LLApplication> ApplicationCreated { get; set; }

        public StartupItemsWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = new StartupItemsViewModel();
            viewModel.StartupItems = new ObservableCollection<StartupItemViewModel>();
            StartupItem.GetStartupItems().ToList().ForEach(x => viewModel.StartupItems.Add(new StartupItemViewModel() { StartupApp = x }));
            this.DataContext = viewModel;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as StartupItemsViewModel;
            foreach (var selectedItem in viewModel.StartupItems.Where(x => x.StartupApp.IsChecked))
            {
                var newApp = selectedItem.StartupApp.CreateApp();
                if (ApplicationCreated != null && newApp != null)
                {
                    // the parent window should deal with adding the app to the config file
                    ApplicationCreated(newApp);
                }
            }
            this.Close();
        }        
    }    
}
