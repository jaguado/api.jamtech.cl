using JAMTech.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JAMTech.Downloadables.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DownloadCollection : Collection<DownloadResult>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DownloadCollection Clone()
        {
            return Clone(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadCollection"></param>
        /// <returns></returns>
        public DownloadCollection Clone(DownloadCollection downloadCollection)
        {
            var clonedDownloadCollection = new DownloadCollection();

            // Deep copy the collection instead of copying the reference with MemberwiseClone()
            foreach (var download in downloadCollection)
            {
                clonedDownloadCollection.Add(download);
            }

            return clonedDownloadCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadCollection"></param>
        /// <returns></returns>
        public DownloadCollection Clone(IEnumerable<DownloadResult> downloadCollection)
        {
            var clonedDownloadCollection = new DownloadCollection();

            // Deep copy the collection instead of copying the reference with MemberwiseClone()
            foreach (var download in downloadCollection)
            {
                clonedDownloadCollection.Add(download);
            }

            return clonedDownloadCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void AddRange(DownloadCollection data)
        {
            data.AsParallel().ForAll(t => this.Add(t));
        }
    }
}
