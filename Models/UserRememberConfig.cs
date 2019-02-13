using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    /// <summary>
    /// Represent a collection of configurations of a user
    /// </summary>
    public class UserRememberConfig
    {
        public string uid { get; set; }
        public IEnumerable<RememberConfig> Data { get; set; }

        public id _id { get; set; }
        public class id
        {
            [JsonProperty("$oid")]
            public string oid { get; set; }
        }
    }
}
