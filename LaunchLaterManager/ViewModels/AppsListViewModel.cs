﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace LaunchLaterManager.ViewModels
{
    public class AppsListViewModel
    {
        public ObservableCollection<AppViewModel> Applications { get; set; }

    }
}