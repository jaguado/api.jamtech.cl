using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class LoggedUser
    {
        public id _id { get; set; }
        public class id
        {
            [JsonProperty("$oid")]
            public string oid { get; set; }
        }
        public dynamic AppInfo {get;set;}
        public dynamic UserInfo {get;set;}
        public DateTime Date{get;set;}
    }
}