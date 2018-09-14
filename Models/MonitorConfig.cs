using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class MonitorConfig
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public AvailableMethods Method { get; set; } = AvailableMethods.GET;
        public int Interval { get; set; }
        public int ExpectedStatusCode { get; set; }
        public string ExpectedResponseBodyContains { get; set; }

        public long ErrDuration { get; set; }
        public long WrnDuration { get; set; }

        public DateTime CreationDate { get; set; }

        public MonitorConfig NextConfig { get; set; }

        public bool Enabled { get; set; } = true;

        public enum AvailableMethods
        {
            GET, POST
        }
    }
}
