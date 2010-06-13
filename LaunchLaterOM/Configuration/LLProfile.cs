using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;

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
        public static List<LLProfile> GetProfiles(string configFileName)
        {
            XDocument xml = XDocument.Load(LLUtilities.GetConfigPath() + "\\" + configFileName);

            var profiles = from p in xml.Element("LaunchLaterConfig").Element("Profiles").Elements()
                                select new LLProfile
                                {
                                    Applications = LLProfile.GetApplications(configFileName, p.Attribute("Name").Value)
                                    ,IsDefault = Convert.ToBoolean(p.Attribute("Default").Value)
                                    ,Name = p.Attribute("Name").Value
                                };

            return profiles.ToList();
            
        }

        public static List<LLApplication> GetApplications(string configFileName, string profileName)
        {
            EventLog.WriteEntry("LaunchLater", "Loading configuration...");

            try
            {

                XDocument xml = XDocument.Load(LLUtilities.GetConfigPath() + "\\" + configFileName);

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
                               Arguments = a.Attribute("Arguments").Value
                           };

                return apps.ToList();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("LaunchLater", "Error loading configuration... " + ex.ToString());
                throw new LLConfigurationException();
            }
        }

    }
}
