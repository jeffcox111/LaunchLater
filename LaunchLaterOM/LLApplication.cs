using System.Xml.Linq;

namespace LaunchLaterOM
{
    public class LLApplication
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public int DelaySeconds { get; set; }
        public string Arguments { get; set; }
        public bool Enabled { get; set; }

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
            return xml;
        }

        
    }

    
}
