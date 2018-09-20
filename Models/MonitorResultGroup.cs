using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class MonitorResultGroup
    {
        public MonitorConfig Config { get; set; }
        public IEnumerable<MonitorResult> Results { get; set; }
        public MonitorResultGroup(MonitorConfig config, IEnumerable<MonitorResult> results)
        {
            Config = config;
            Results = results;
        }
    }
}
