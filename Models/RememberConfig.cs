using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class RememberConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastRun { get; set; }
        public enum RecurrenceTypes
        {
            Minutes, Hours, Days, Weeks, Months, Years
        }
        public bool Enabled { get; set; } = true;
        public int RecurrenceValue { get; set; }
        public RecurrenceTypes RecurrenceType { get; set; }
    }
}
