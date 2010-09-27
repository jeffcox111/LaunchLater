using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace LaunchLaterManager.ViewModels
{
    public class AppsListViewModel
    {
        public ObservableCollection<AppViewModel> Applications { get; set; }

        public void SortApps(AppSortingStyle style)
        {
            ObservableCollection<AppViewModel> apps = new ObservableCollection<AppViewModel>();

            switch (style)
            {
                case AppSortingStyle.Name:

                    apps = new ObservableCollection<AppViewModel>((from a in Applications.AsEnumerable()
                            orderby a.App.Name ascending
                            select a));

                    break;
                case AppSortingStyle.Timeline:

                    apps = new ObservableCollection<AppViewModel>((from a in Applications.AsEnumerable()
                                                                   orderby a.App.DelaySeconds ascending
                                                                   select a));

                    break;
                case AppSortingStyle.Enabled:

                    apps = new ObservableCollection<AppViewModel>((from a in Applications.AsEnumerable()
                                                                   orderby a.App.Enabled descending
                                                                   select a));

                    break;
            }

            Applications = apps;

        }
    }

    public enum AppSortingStyle
    {
        Name,
        Timeline,
        Enabled
    }
}
