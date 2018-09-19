using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class MonitorConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public AvailableMethods Method { get; set; } = AvailableMethods.GET;
        public int Interval { get; set; }
        public int ExpectedStatusCode { get; set; }
        public string ExpectedResponseBodyContains { get; set; }

        public long ErrDuration { get; set; }
        public long WrnDuration { get; set; }

        public DateTime CreationDate { get; set; }  

        public int ResultsSizeLimit { get; set; } = 50;
        public bool Enabled { get; set; } = true;

        public enum AvailableMethods
        {
            GET, POST
        }
    }
}
