using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class SavedPassword
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Url { get; set; }
        public string Comments { get; set; }


        public SavedPassword()
        {
            Id = new Guid().ToString();
        }
    }
    
}
