using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class SavedPassword
    {
        private string _password;

        public string Id { get; set; }
        public string Source { get; set; }
        public string Username { get; set; }
        public string Password { get => null; set => _password = value; }
        public string GetPassword()
        {
            return _password;
        }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Url { get; set; }
        public string Comments { get; set; }

        

        public SavedPassword()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
