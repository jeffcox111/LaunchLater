using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using LaunchLaterOM;

namespace LaunchLaterManager.ViewModels
{
    public class StartupItemsViewModel
    {
        public ObservableCollection<StartupItem> StartupItems { get; set; }

        public StartupItemsViewModel()
        {
            StartupItems = new ObservableCollection<StartupItem>();
        }
    }
}
