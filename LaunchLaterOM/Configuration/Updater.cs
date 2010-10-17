using System.Collections.Generic;
using System.Linq;
using LinqToFeed;

namespace LaunchLaterOM.Configuration
{
    public static class Updater
    {
        public static bool UpdateExists(double currentVersion)
        {
            if (currentVersion < getLatestReleaseVersion())
                return true;
            else
                return false;
        }

        private static double getLatestReleaseVersion()
        {
            try
            {
                List<Feed> feed = RSSConnector.getBlogInfo("http://launchlater.codeplex.com/Project/ProjectRss.aspx?ProjectRSSFeed=codeplex://release/launchlater");

                string txt = feed.Where(x => x.Title.StartsWith("Released")).First().Title.Split().ToList()[2];
                double result = 0;
                double.TryParse(txt, out result);
                return result;
            }
            catch
            {
                return 0;
            }
 
        }
    }
}
