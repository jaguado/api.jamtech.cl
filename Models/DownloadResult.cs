namespace JAMTech.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DownloadResult
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public string Url { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public string BaseUrl { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDirectory { get
            {
                return Url != null && Url.EndsWith("/");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DownloadResult ShallowCopy()
        {
            return (DownloadResult)this.MemberwiseClone();
        }
    }
}
