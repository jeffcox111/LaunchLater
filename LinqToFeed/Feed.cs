using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToFeed {
    public class Feed {

        public string Title { get; set; }
        public string PublishDate {get; set;}
        public string Content { get; set; }
        public string Url { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string Description { get; set; }

    }
}
