using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchLaterOM
{
    public class StartupRegistryInformation
    {
        public string RegistryName { get; set; }
        public string RegistryLocation { get; set; }
        public string RegistryKey { get; set; }
        public string RegistryValue { get; set; }

        public StartupRegistryInformation(string name, string location, string key, string value)
        {
            RegistryName = name;
            RegistryLocation = location;
            RegistryKey = key;
            RegistryValue = value;
        }
    }
}
