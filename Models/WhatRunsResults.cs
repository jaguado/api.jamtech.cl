using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class WhatRunsResults
    {
        public bool status { get; set; }
        public string apps { get; set; }

        public List<Section> GetSections()
        {
            var result = new List<Section>();
            var results = JsonConvert.DeserializeObject<JToken>(apps);
            foreach (JProperty pages in results)
            {
                foreach (JProperty section in pages.First)
                {
                    result.Add(new Section
                    {
                        name = section.Name,
                        detail = JsonConvert.DeserializeObject<Detail[]>(section.Value.ToString())
                    });
                }
            }
            return result;
        }

        public class Section
        {
            public string name { get; set; }
            public Detail[] detail { get; set; }
        }

        public class Detail
        {
            public string name { get; set; }
            public int category { get; set; }
            public string icon { get; set; }
            public string index { get; set; }
            public string version { get; set; }
            public string sourceUrl { get; set; }
            public long detectedTime { get; set; }
            public long latestDetectedTime { get; set; }
            public string website { get; set; }
            public string siteListUrl { get; set; }
        }
    }



}