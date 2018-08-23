using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class Price
    {
        public int local_id { get; set; }
        public string price_str { get; set; }
        public int price { get; set; }
        public string ppu { get; set; }
        public string status_load { get; set; }
    }

    public class Product
    {
        public int product_id { get; set; }
        public string product_type { get; set; }
        public string brand { get; set; }
        public string image { get; set; }
        public string thumb { get; set; }
        public List<Price> price { get; set; }
        public string sale_unit { get; set; }
        public string description { get; set; }
        public double maximum { get; set; }
        public double interval { get; set; }
        public bool weighable { get; set; }
        public string description_banner { get; set; }
        public string banner_name { get; set; }
        public int sap_id { get; set; }
        public List<object> labels { get; set; }
        public object food_label_icons { get; set; }
        public List<object> offer_icons { get; set; }
        public int? order { get; set; }
        public bool published { get; set; }
        public List<object> offers { get; set; }
        public string status_load { get; set; }
        public bool active { get; set; }
        public List<string> category { get; set; }
        public List<string> subcategory { get; set; }
        public List<int> category_ids { get; set; }
        public List<int> subcategory_ids { get; set; }
    }
}
