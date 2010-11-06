using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchLaterOM
{
    public class StartupRegistryItem : StartupItem
    {
        public string RegistryName { get; set; }
        public string RegistryLocation { get; set; }
        public string RegistryKey { get; set; }
        public string RegistryValue { get; set; }

        public override string Name
        {
            get { return RegistryKey; }
        }

        public StartupRegistryItem(string fileName, string registryItem, string registryValue, string registryName, string registryLocation, string args)
            : base(fileName, args)
        {
            RegistryKey = registryItem;
            RegistryValue = registryValue;
            RegistryName = registryName;
            RegistryLocation = registryLocation;
            IsChecked = ShouldBeChecked();
        }
    }
}
