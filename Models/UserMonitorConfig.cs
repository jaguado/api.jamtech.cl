using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    /// <summary>
    /// Represent a collection of configurations of a user
    /// </summary>
    public class UserMonitorConfig
    {
        public string uid { get; set; }
        public IEnumerable<Models.MonitorConfig> Data { get; set; }

        public id _id { get; set; }
        public class id
        {
            [JsonProperty("$oid")]
            public string oid { get; set; }
        }
    }
}
