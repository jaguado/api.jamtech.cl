using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class JumboLocals
    {
        public List<Local> locals { get; set; }
        public class Local
        {
            public int local_id { get; set; }
            public string local_code { get; set; }
            public string local_name { get; set; }
            public bool click_collect { get; set; }
            public string address { get; set; }
            public string status_load { get; set; }
            public string delivery_status { get; set; }
        }
    }
}
