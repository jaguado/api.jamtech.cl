using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech.Models
{
    /// <summary>
    /// Result of torrent search
    /// </summary>
    public class TorrentResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int Position { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public int Page { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// Torrent name
        /// </summary>
        public string Type { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public string SubType { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public string Link { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public bool Vip { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Description { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public List<Tuple<string,string>> Links { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public int Seeds { set; get; }
        /// <summary>
        /// Seeds availables
        /// </summary>
        public int Leeds { set; get; }

        /// <summary>
        /// Leeds avaiables
        /// </summary>
        public TorrentResult()
        {
            Description = new List<string>();
            Links = new List<Tuple<string, string>>();
        }
        
        
    }
}
