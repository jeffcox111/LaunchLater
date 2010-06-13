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

namespace LaunchLaterManager
{
    /// <summary>
    /// Interaction logic for AppListBox.xaml
    /// </summary>
    public partial class AppListBox : UserControl
    {
        public AppListBox()
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
    }
}
