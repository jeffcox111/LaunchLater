﻿using LaunchLaterManager.ViewModels;
using LaunchLaterOM;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public delegate void DeleteAppHandler(object sender, AppDeletedEventArgs e);
        public event DeleteAppHandler OnAppDeleted;
        
        public AppView()
		{
			this.InitializeComponent();
           
		}

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "EditingMode", true);
            tempApp = new LLApplication(((AppViewModel)DataContext).App);
        }

        public LLApplication App
        {
            get
            {
                if (DataContext is AppViewModel)
                    return ((AppViewModel)DataContext).App;
                else
                    return new LLApplication();
            }
            private set{}
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            GoToViewingMode();

            if (OnChangeHasBeenMade != null)
            {
                AppViewModel currentApp = (AppViewModel)this.DataContext;
                currentApp.Arguments = ArgumentsText.Text;
                currentApp.DelaySeconds = (int.Parse(cmbSeconds.Text) + (60 * int.Parse(cmbMinutes.Text))).ToString();
                currentApp.Enabled = EnabledCheckBox.IsChecked ?? true;
                OnChangeHasBeenMade(this, new EventArgs());
            }

            GoToViewingMode();
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

                AppViewModel currentApp = (AppViewModel)this.DataContext;
                currentApp.Name = ofd.SafeFileName.Remove(ofd.SafeFileName.Length - 4, 4);
                currentApp.FullPath = ofd.FileName;

                this.DataContext = currentApp;
            }
            else
            {
                AppViewModel currentApp = (AppViewModel)this.DataContext;
                if (currentApp.Name == null || currentApp.Name == "")
                {
                    OnAppDeleted(this, new AppDeletedEventArgs(false));
                }
            }
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this application?", "LaunchLater", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var shouldRestore = false;
//                if (App.IsImported)
                    shouldRestore = MessageBox.Show("Do you want LaunchLater to put this application back from where it was originally imported?\n (Non-Imported Apps go to the Startup Folder)", "LaunchLater", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

                OnAppDeleted(this, new AppDeletedEventArgs(shouldRestore));
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
            DataContext = new AppViewModel() { App = tempApp };

            GoToViewingMode();
        	
        }

        public void GoToViewingMode()
        {
            if (App.Enabled == true)
                VisualStateManager.GoToState(this, "ViewingMode", true);
            else
                VisualStateManager.GoToState(this, "ViewingDisabledMode", true);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            for (int i = 0; i < 61; i++)
                cmbMinutes.Items.Add(i.ToString());
                            
            for (int i = 0; i < 60; i++)
                cmbSeconds.Items.Add(i.ToString());

			if(IsNewAppConfig && App.FullPath=="")
			{
                EditButton_Click(this, new RoutedEventArgs());
                FindAppButton_Click(this, new RoutedEventArgs());
                GoToViewingMode();
			}
        	
            IsNewAppConfig = false;
            
        }

       

       
	}

    public class AppDeletedEventArgs
    {
        public bool ShouldRestoreToStartupItems { get; set; }

        public AppDeletedEventArgs(bool shouldRestoreToStartupItems)
        {
            ShouldRestoreToStartupItems = shouldRestoreToStartupItems;
        }
    }
}