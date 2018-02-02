using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration
{
    public class HelpLinkAttribute : Attribute
    {
        public string Url { get; }
        public string Text { get; set; }
        public string Description { get; set; }

        public HelpLinkAttribute(string url)
        {
            Url = url;
        }
    }
}
