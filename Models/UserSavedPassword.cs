using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    /// <summary>
    /// Represent a collection of saved passwords of a user
    /// </summary>
    public class UserSavedPassword
    {
        public string uid { get; set; }
        public IEnumerable<SavedPassword> Data { get; set; }

        public id _id { get; set; }
        public class id
        {
            [JsonProperty("$oid")]
            public string oid { get; set; }
        }
    }
}
