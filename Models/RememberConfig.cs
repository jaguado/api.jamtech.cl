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
        public string Description { get; set; }
        public List<string> RememberTo { get; set; }
        public List<Tuple<RecurrenceTypes, int>> RememberEarlier { get; set; }
        public RememberType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastRun { get; set; }
        public enum RecurrenceTypes
        {
            Minutes, Hours, Days, Weeks, Months, Years
        }
        public bool Enabled { get; set; } = true;
        public int RecurrenceValue { get; set; }
        public RecurrenceTypes RecurrenceType { get; set; }

        public enum RememberType
        {
            Email, SMS, Custom
        }

        public string TemplateId { get; set; } = "d124da8a-f784-4410-8e5f-6d11a821130e";
        public IDictionary<string, string> Substitutions { get; set; }
    }
}
