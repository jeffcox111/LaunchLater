using LaunchLaterOM;
using System.Collections.ObjectModel;

namespace LaunchLaterManager.ViewModels
{
    public class StartupItemsViewModel
    {
        public ObservableCollection<StartupItemViewModel> StartupItems { get; set; }

        public StartupItemsViewModel()
        {
            StartupItems = new ObservableCollection<StartupItemViewModel>();
        }


    }
}
