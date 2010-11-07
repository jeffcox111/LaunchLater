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
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using LaunchLaterOM;
using LaunchLaterManager.ViewModels;

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
            viewModel.StartupItems = new ObservableCollection<StartupItem>(StartupItem.GetStartupItems());

            this.DataContext = viewModel;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as StartupItemsViewModel;
            foreach (var selectedItem in viewModel.StartupItems.Where(x => x.IsChecked))
            {
                var newApp = selectedItem.CreateApp();
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
