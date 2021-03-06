﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace LaunchLaterOM.Configuration
{
    public class LLProfile
    {
        public string Name { get; set; }
        public List<LLApplication> Applications;
        public bool IsDefault { get; set; }

        LLProfile()
        {

        }

        public XElement ToXML()
        {
            XElement xml = new XElement("Profile", new XAttribute("Name", this.Name));
            XAttribute defaultSetting = new XAttribute("Default", IsDefault.ToString());
            xml.Add(defaultSetting);

            XElement apps = new XElement("LLApplications");
            Applications.ForEach(x => apps.Add(x.ToXML()));

            xml.Add(apps);
            return xml;
        }

        public override string ToString()
        {
            return this.ToXML().ToString();
        }
        public static List<LLProfile> GetProfiles()
        {
            XDocument xml;

            if (File.Exists(LLConfiguration.FILENAME))
                xml = XDocument.Load(LLConfiguration.FILENAME);
            else
                xml = XDocument.Load(LLConfiguration.OLDFILENAME);

            var profiles = from p in xml.Element("LaunchLaterConfig").Element("Profiles").Elements()
                                select new LLProfile
                                {
                                    Applications = LLProfile.GetApplications(p.Attribute("Name").Value)
                                    ,IsDefault = Convert.ToBoolean(p.Attribute("Default").Value)
                                    ,Name = p.Attribute("Name").Value
                                };

            return profiles.ToList();
            
        }

        public static List<LLApplication> GetApplications(string profileName)
        {
            EventLog.WriteEntry("LaunchLater", "Loading configuration...");

            try
            {
                XDocument xml;

                if (File.Exists(LLConfiguration.FILENAME))
                    xml = XDocument.Load(LLConfiguration.FILENAME);
                else
                    xml = XDocument.Load(LLConfiguration.OLDFILENAME);
                

                XElement defaultProfile = (from a in xml.Element("LaunchLaterConfig").Element("Profiles").Elements()
                                           where a.Attribute("Name").Value == profileName
                                           select a).First();

                var apps = from a in defaultProfile.Element("LLApplications").Elements()
                           where a.Name == "LLApplication"
                           select new LLApplication
                           {
                               FullPath = a.Attribute("FullPath").Value,
                               Name = a.Attribute("Name").Value,
                               DelaySeconds = int.Parse(a.Attribute("DelaySeconds").Value),
                               Arguments = a.Attribute("Arguments").Value,
                               Enabled = getSafeBoolean(a, "Enabled"),
                               FolderInfo = GetFolderInfo(a),
                               RegistryInfo = GetRegistryInfo(a)
                           };

                return apps.ToList();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("LaunchLater", "Error loading configuration... " + ex.ToString());
                throw new LLConfigurationException();
            }
        }

        private static StartupFolderInformation GetFolderInfo(XElement a)
        {
            var shortcutAttr = a.Attribute("ShortcutFullPath");
            if(shortcutAttr == null || string.IsNullOrEmpty(shortcutAttr.Value)) return null;
            return new StartupFolderInformation(shortcutAttr.Value);
        }

        private static StartupRegistryInformation GetRegistryInfo(XElement a)
        {
            var regNameAttr = a.Attribute("RegistryName");
            var regLocationeAttr = a.Attribute("RegistryLocation");
            var regKeyAttr = a.Attribute("RegistryKey");
            var regValueAttr = a.Attribute("RegistryValue");
            // make sure none of the values are missing or non-existant
            if (regNameAttr == null || regLocationeAttr == null || regKeyAttr == null || regValueAttr == null) return null;
            if (string.IsNullOrEmpty(regNameAttr.Value) || string.IsNullOrEmpty(regLocationeAttr.Value) || string.IsNullOrEmpty(regKeyAttr.Value) || string.IsNullOrEmpty(regValueAttr.Value)) return null;

            return new StartupRegistryInformation(regNameAttr.Value, regLocationeAttr.Value, regKeyAttr.Value, regValueAttr.Value);
        }

        private static bool getSafeBoolean(XElement a, string p)
        {
            try
            {                
                bool result = a.Attribute(p) != null ? bool.Parse(a.Attribute(p).Value) : true;
                return result;
            }
            catch
            {
                return true;
            }
        }

    }
}
