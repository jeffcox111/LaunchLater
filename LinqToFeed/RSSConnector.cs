using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LinqToFeed {
    
    public static class RSSConnector 
    {
         static RSSConnector() { }
         
        public static List<Feed> getBlogInfo(string url)
        {
            List<Feed> results = new List<Feed>();
            FeedAdapter adapter;
            XDocument xmlFeed;
            try
            {

                xmlFeed = XDocument.Load(url);
                if (xmlFeed.ToString().Contains(@"xmlns:content=""http://purl.org/rss/1.0/modules/content/"""))
                {
                    adapter = new RSS2Adapter();
                }
                else if (xmlFeed.ToString().Contains(@"xmlns=""http://www.w3.org/2005/Atom"""))
                {
                    adapter = new AtomAdapter();
                }
                else
                {
                    //default
                    adapter = new RSS2Adapter();
                }
                
                results = adapter.getBlogs(xmlFeed);
                return results;
                
            }
            catch(Exception ex)
            {
                throw new Exception("Could not parse feed at URL: " + url + ".  " + ex);
            }
                            
                          
        }

        public static List<Feed> getBlogInfo(string url, int numberOfPosts) {

            return getBlogInfo(url).Take(numberOfPosts).ToList();;
        }

        public static List<Feed> getBlogInfo(string url, string category) {
            
           
            var tempresults = from blog in getBlogInfo(url)
                              where blog.Categories.Contains(category)
                              select blog;
            
            return tempresults.ToList();
        }

        public static List<Feed> getBlogInfo(string url, string category, int numberOfPosts) {
            return getBlogInfo(url, category).Take(numberOfPosts).ToList();
        }
        
    }

}
