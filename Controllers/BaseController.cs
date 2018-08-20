using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace JAMTech.Controllers
{
    public abstract class BaseController : Controller
    {
        public string[] Operators = new[] { "==", "!=", "<", ">", "<>", "<=", ">=" };

        /// <summary>
        /// Allows cors support
        /// </summary>
        /// <returns></returns>
        [HttpOptions]
        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        internal IActionResult HandleException(Exception ex)
        {
            Log(ex);
            return StatusCode(500, ex.Message);
        }

        internal IActionResult HandleWebException(WebException ex)
        {
            Log(ex);
            return StatusCode((int)ex.Status, ex.Message);
        }

        internal void Log(Exception ex)
        {
            var msg = $"{DateTime.Now.ToString()}|ERROR|{ex.Source}|{ex.Message}|{ex.StackTrace}";
            Console.Error.WriteLineAsync(msg);
        }

        internal IEnumerable<T> OrderResult<T>(IEnumerable<T> filteredResult)
        {
            var order = Request.Query["order"];
            if (order.Any())
                foreach (var o in order)
                    filteredResult = filteredResult.AsQueryable().OrderBy(o);
            return filteredResult;
        }

        internal IEnumerable<T> FilterResult<T>(IEnumerable<T> filteredResult)
        {
            var filters = Request.Query["filters"];
            if (filters.Any())
            {
                var query = BuildQueryFromRequest(filters, out List<object> values);
                filteredResult = filteredResult.AsQueryable().Where(query, values.ToArray());
            }
            return filteredResult;
        }
        private string BuildQueryFromRequest(Microsoft.Extensions.Primitives.StringValues filters, out List<object> values)
        {
            var query = "";
            values = new List<object>();
            foreach (var filter in filters)
            {
                foreach (var op in Operators)
                {
                    var args = filter.Split(op);
                    if (args.Length > 1)
                    {
                        var field = args[0];
                        var value = args[1];
                        if (query != string.Empty)
                            query += " and ";
                        query += $"{field} {op} @{values.Count}";
                        //check if numeric
                        if (int.TryParse(value, out int newValue))
                            values.Add(newValue);
                        else
                            values.Add(value);
                    }
                }
            }
            return query;
        }

    }
}