using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class KnastaSearchResullt
    {
        public int page { get; set; }
        public int total_pages { get; set; }
        public List<P> p { get; set; }
        public int count { get; set; }
        public string order { get; set; }
        public List<Product> products { get; set; }
        public List<Ktegory> ktegories { get; set; }
        public string q { get; set; }
        public object k { get; set; }



        public class P
        {
            public string value { get; set; }
            public string label { get; set; }
        }

        public class Spec
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        public class Product
        {
            public int is_premium { get; set; }
            public int ktype { get; set; }
            public double price_value { get; set; }
            public string images { get; set; }
            public string sku { get; set; }
            public string kategory { get; set; }
            public string kid { get; set; }
            public string title { get; set; }
            public int percent { get; set; }
            public string retail { get; set; }
            public string product_id { get; set; }
            public string price { get; set; }
            public double price_tm { get; set; }
            public int days_old { get; set; }
            public List<Spec> specs { get; set; }
        }

        public class Ktegory
        {
            public int value { get; set; }
            public string label { get; set; }
        }
    }
}
