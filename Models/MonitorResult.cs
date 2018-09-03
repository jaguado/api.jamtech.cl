using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class MonitorResult
    {
        public MonitorResult(DateTime date, bool success, string msg)
        {
            Date = date;
            Success = success;
            ErrorMessage = msg;
        }
        public DateTime Date { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long Duration { get; set; }
    }
}
