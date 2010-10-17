using System;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LaunchLaterManager
{
    /// <summary>
    /// Interaction logic for AppsListView.xaml
    /// </summary>
    public partial class AppsListView : UserControl
    {
        public AppsListView()
        {
            InitializeComponent();
        }

        public delegate void ChangeHasBeenMadeHandler(object sender, EventArgs e);
        public event ChangeHasBeenMadeHandler OnChangeHasBeenMade;

        public delegate void DeleteAppHandler(object sender, EventArgs e);
        public event DeleteAppHandler OnAppDeleted;

        private void AppView_OnChangeHasBeenMade(object sender, EventArgs e)
        {
            OnChangeHasBeenMade(this, new EventArgs());
        }

        private void AppView_OnAppDeleted(object sender, EventArgs e)
        {
            OnAppDeleted(sender, new EventArgs());
        }

        private void AppView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((AppView)sender).GoToViewingMode();
        }

           
    }
}
