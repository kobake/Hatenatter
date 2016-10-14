using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hatenatter.Models
{
    public class TimelineResponse
    {
        public string Result { get; set; }
        public string Error { get; set; }
        public string MyUserId { get; set; }
        public List<TimelineItem> Timeline { get; set; }
    }
}
