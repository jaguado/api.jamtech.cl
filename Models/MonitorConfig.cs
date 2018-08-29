using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class MonitorConfig
    {
        public string Url { get; set; }
        public AvailableMethods Method { get; set; }
        public int Interval { get; set; }
        public int ExpectedStatusCode { get; set; }
        public string ExpectedResponseBodyContains { get; set; }


        public enum AvailableMethods
        {
            GET, POST
        }
    }
}
