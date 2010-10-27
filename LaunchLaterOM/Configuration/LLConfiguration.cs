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

        public static string FILENAME = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\LaunchLater\\LaunchLaterApps.config";
        public static string OLDFILENAME = "LaunchLaterApps.config";

        public LLProfile DefaultProfile
        {
            get
            {
                return Profiles.Where(x => x.IsDefault == true).First();
            }
            private set{}
        }
       

        public LLConfiguration(bool autoload)
        {
            Profiles = LLProfile.GetProfiles();

            IsDirty = false;
        }

        public override string ToString()
        {
            XElement profs = new XElement("Profiles");

            Profiles.ForEach(x => profs.Add(x.ToXML()));
            XDocument xml = new XDocument(new XElement("LaunchLaterConfig", profs));

            return xml.ToString();
        }


        public bool WriteFreshConfigurationFile()
        {
            try
            {
                XElement profs = new XElement("Profiles");

                Profiles.ForEach(x => profs.Add(x.ToXML()));
                XDocument xml = new XDocument(new XElement("LaunchLaterConfig", profs));
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString();

                if (!Directory.Exists(path + "\\LaunchLater"))
                    Directory.CreateDirectory(path + "\\LaunchLater");

                xml.Save(FILENAME);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("LaunchLater", "There was a problem saving configuration to the file: LaunchLaterApps.config" + ". " + ex.ToString());
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
