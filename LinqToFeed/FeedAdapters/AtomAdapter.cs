using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace LinqToFeed
{
    class AtomAdapter:FeedAdapter
    {
        #region FeedAdapter Members

        public List<Feed> getBlogs(System.Xml.Linq.XDocument xmlFeed)
        {
            List<Feed> results = new List<Feed>();
            XNamespace xmlns = "http://www.w3.org/2005/Atom";

            var tmpresults = from item in xmlFeed.Descendants(xmlns + "entry")
                             select new Feed
                             {
                                 Title = item.Element(xmlns + "title").Value,
                                 PublishDate = item.Element(xmlns + "published").Value,
                                 Url = (from l in item.Elements(xmlns + "link")
                                       where l.Attribute("rel").Value == "alternate"
                                       select l.Attribute("href").Value).First(),
                                 Content = item.Element(xmlns + "content").Value,
                                 Categories = from c in item.Elements(xmlns + "category")
                                              select c.Attribute("term").Value
                             };

            results = tmpresults.ToList();

            return results;
        }

        #endregion
    }
}
