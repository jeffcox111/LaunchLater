using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace LaunchLaterOM
{
    public class LLApplication
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public int DelaySeconds { get; set; }
        public string Arguments { get; set; }

        public LLApplication() { }

        public LLApplication(LLApplication app)
        {
            Name = app.Name;
            FullPath = app.FullPath;
            DelaySeconds = app.DelaySeconds;
            Arguments = app.Arguments;
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
            return xml;
        }

        
    }

    
}
