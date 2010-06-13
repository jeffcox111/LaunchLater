﻿using System;
using System.Collections.Generic;
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
using LaunchLaterOM;
using LaunchLaterManager.ViewModels;

namespace LaunchLaterManager
{
	/// <summary>
	/// Interaction logic for AppView.xaml
	/// </summary>
	public partial class AppView : UserControl
	{

        private LLApplication tempApp;
        private bool IsNewAppConfig = true;
		
        public delegate void ChangeHasBeenMadeHandler(object sender, EventArgs e);
        public event ChangeHasBeenMadeHandler OnChangeHasBeenMade;

        public delegate void DeleteAppHandler(object sender, EventArgs e);
        public event DeleteAppHandler OnAppDeleted;
        
        public AppView()
		{
			this.InitializeComponent();
            VisualStateManager.GoToState(this, "ViewingMode", true);
		}

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "EditingMode", true);
            tempApp = new LLApplication(((LLApplicationViewModel)DataContext).App);
        }

        public LLApplication App
        {
            get
            {
                return ((LLApplicationViewModel)DataContext).App;
            }
            private set{}
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ViewingMode", true);
            if(OnChangeHasBeenMade != null)
                OnChangeHasBeenMade(this, new EventArgs());
        }

        private void FindAppButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Executables|*.exe";
            ofd.ShowDialog();

            if (ofd.FileName != null && ofd.FileName != "")
            {
                if (ofd.FileName.EndsWith("LL_LaunchPad.exe"))
                {
                    MessageBox.Show("Now that's just silly...");
                    return;
                }

                LLApplicationViewModel currentApp = (LLApplicationViewModel)this.DataContext;
                currentApp.Name = ofd.SafeFileName.Remove(ofd.SafeFileName.Length - 4, 4);
                currentApp.FullPath = ofd.FileName;

                this.DataContext = currentApp;
            }
            else
            {
                OnAppDeleted(this, new EventArgs());
            }

            
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this application?", "LaunchLater", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                OnAppDeleted(this, new EventArgs());
            }
            
        }

        private void DeleteButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "DeleteHoverMode", false);
        }

        private void DeleteButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "EditingMode", false);
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataContext = new LLApplicationViewModel() { App = tempApp };

            VisualStateManager.GoToState(this, "ViewingMode", true);
        	// TODO: Add event handler implementation here.
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
			if(IsNewAppConfig && App.FullPath=="")
			{
                EditButton_Click(this, new RoutedEventArgs());
                FindAppButton_Click(this, new RoutedEventArgs());
			}
        	IsNewAppConfig = false;
			
        }

       

       
	}
}