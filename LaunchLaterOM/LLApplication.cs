using System.Xml.Linq;

namespace LaunchLaterOM
{
    public class LLApplication
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string Arguments { get; set; }
        public bool Enabled { get; set; }
        public StartupRegistryInformation RegistryInfo { get; set; }
        public StartupFolderInformation FolderInfo { get; set; }
        public bool IsImported { get { return RegistryInfo != null || FolderInfo != null; } }

        private int delaySeconds;
        public int DelaySeconds
        {
            get { return delaySeconds; }
            set
            {
                if (value == 0)
                {
                    delaySeconds = 1;
                }
                else
                {
                    delaySeconds = value;
                }
            }
        }
            

        public LLApplication() { }

        public LLApplication(LLApplication app)
        {
            Name = app.Name;
            FullPath = app.FullPath;
            DelaySeconds = app.DelaySeconds;
            Arguments = app.Arguments;
            Enabled = app.Enabled;
        }
        public override string ToString()
        {
            return ToXML().ToString();
        }

        public XElement ToXML()
        {
            XElement xml = new XElement("LLApplication");
            xml.Add(new XAttribute("Name", Name));
            xml.Add(new XAttribute("FullPath", FullPath));
            xml.Add(new XAttribute("DelaySeconds", DelaySeconds.ToString()));
            xml.Add(new XAttribute("Arguments", Arguments));
            xml.Add(new XAttribute("Enabled", Enabled.ToString()));
            if (RegistryInfo != null)
            {
                xml.Add(new XAttribute("RegistryName", RegistryInfo.RegistryName),
                        new XAttribute("RegistryLocation", RegistryInfo.RegistryLocation),
                        new XAttribute("RegistryKey", RegistryInfo.RegistryKey),
                        new XAttribute("RegistryValue", RegistryInfo.RegistryValue));
            }
            if (FolderInfo != null)
            {
                xml.Add(new XAttribute("ShortcutFullPath", FolderInfo.ShortcutFullPath));
            }
            return xml;
        }
    }

    
}
