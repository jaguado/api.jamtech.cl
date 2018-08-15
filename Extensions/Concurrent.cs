using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JAMTech.Extensions
{
    /// <summary>
    /// Concurrent Collections Extensions
    /// </summary>
    public static class Concurrent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this ConcurrentBag<T> @this, List<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this ConcurrentBag<T> @this, ConcurrentBag<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this ConcurrentBag<T> @this, Collection<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this List<T> @this, ConcurrentBag<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this List<T> @this, List<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="toAdd"></param>
        public static void AddRange<T>(this List<T> @this, IEnumerable<T> toAdd)
        {
            toAdd.AsParallel().ForAll(t => @this.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        public static List<T> Clone<T>(this ConcurrentBag<T> @this)
        {
            var toAdd = new ConcurrentBag<T>();
            @this.AsParallel().ForAll(t => toAdd.Add(t));
            return toAdd.ToList();
        }

    }
}
