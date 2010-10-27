using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using LaunchLaterOM.Configuration;
using System.IO;
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
                XElement profs = new XElement("Profiles");

                Profiles.ForEach(x => profs.Add(x.ToXML()));
                XDocument xml = new XDocument(new XElement("LaunchLaterConfig", profs));
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString();

                if (!Directory.Exists(path + "\\LaunchLater"))
                    Directory.CreateDirectory(path + "\\LaunchLater");
                

                fileName = path + "\\LaunchLater\\" + fileName;               
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
