using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using LaunchLaterOM.Configuration;
namespace LaunchLaterOM
{
    public class LLConfiguration : IComparable
    {
        public List<LLProfile> Profiles;
        
        public string configFile { get; set; }

        public bool IsDirty { get; set; }

        public LLProfile DefaultProfile
        {
            get
            {
                return Profiles.Where(x => x.IsDefault == true).First();
            }
            private set{}
        }
       

        public LLConfiguration(string configurationFile)
        {
            configFile = configurationFile;
            
            Profiles = LLProfile.GetProfiles(configFile);

            IsDirty = false;
        }

        public override string ToString()
        {
            XElement profs = new XElement("Profiles");

            Profiles.ForEach(x => profs.Add(x.ToXML()));
            XDocument xml = new XDocument(new XElement("LaunchLaterConfig", profs));

            return xml.ToString();
        }


        public bool WriteFreshConfigurationFile(string fileName)
        {
            try
            {
                //XDocument xml = new XDocument(new XElement("LaunchLaterConfig"));
                XElement profs = new XElement("Profiles");

                Profiles.ForEach(x => profs.Add(x.ToXML()));
                XDocument xml = new XDocument(new XElement("LaunchLaterConfig", profs));
                
                //xml.Add(profs);
                
                xml.Save(fileName);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("LaunchLater", "There was a problem saving configuration to the file: " + fileName + ". " + ex.ToString());
                return false;
            }

        }




        #region IComparable Members

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToString());
        }

        #endregion
    }
}
