using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace LinqToFeed
{
    public class RSS2Adapter : FeedAdapter
    {
        public List<Feed> getBlogs(XDocument xmlFeed)
        {
            List<Feed> results = new List<Feed>();
            XNamespace slashNamespace = "http://purl.org/rss/1.0/modules/content/";

            var tmpresults = from item in xmlFeed.Descendants("item")
                              select new Feed
                              {
                                  Title = item.Element("title").Value,
                                  PublishDate = item.Element("pubDate").Value,
                                  Url = item.Element("link").Value,
                                  //Content = item.Element(slashNamespace.GetName("encoded")).Value
                                  Description = item.Element("description").Value//,
                                 // Categories = from c in item.Elements()
                                   //            where c.Name == "category"
                                     //          select c.Value
                              };

            results = tmpresults.ToList();

            return results;
        }
    }
}
