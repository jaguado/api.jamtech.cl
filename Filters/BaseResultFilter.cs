using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace JAMTech.Filters
{
    public class BaseResultFilter : IActionFilter
    {
        const int defaultLimit = 100;
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // do something before the action executes
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
            var request = context.HttpContext.Request;
            var resultResponse = context.Result as OkObjectResult;
            LimitResult(context, request, resultResponse);
        }

        private void LimitResult(ActionExecutedContext context, HttpRequest request, OkObjectResult resultResponse)
        {
            if (resultResponse != null)
            {
                var result = resultResponse.Value as IEnumerable<object>;
                if (result != null)
                    context.Result = new OkObjectResult(LimitObjectResult(result, request));
            }
        }
        private void OrderResult(ActionExecutedContext context, HttpRequest request, OkObjectResult resultResponse)
        {
            if (resultResponse != null)
            {
                var result = resultResponse.Value as IEnumerable<object>;
                if (result != null)
                    context.Result = new OkObjectResult(OrderResult(result, request));
            }
        }
        private void FilterResult(ActionExecutedContext context, HttpRequest request, OkObjectResult resultResponse)
        {
            if (resultResponse != null)
            {
                var result = resultResponse.Value as IEnumerable<object>;
                if (result != null)
                    context.Result = new OkObjectResult(FilterResult(result, request));
            }
        }
        public static IEnumerable<T> OrderResult<T>(IEnumerable<T> filteredResult, HttpRequest Request)
        {
            var order = Request.Query["order"];
            if (order.Any())
                foreach (var o in order)
                    filteredResult = filteredResult.AsQueryable().OrderBy(o);
            return filteredResult;
        }

        public static IEnumerable<T> FilterResult<T>(IEnumerable<T> filteredResult, HttpRequest Request)
        {
            var filters = Request.Query["filters"];
            if (filters.Any())
            {
                var query = BuildQueryFromRequest(filters, out List<object> values);
                filteredResult = filteredResult.AsQueryable().Where(query, values.ToArray());
            }
            return filteredResult;
        }

        public static string[] Operators = new[] { "==", "!=", "<", ">", "<>", "<=", ">=" };
        private static string BuildQueryFromRequest(Microsoft.Extensions.Primitives.StringValues filters, out List<object> values)
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

        internal IEnumerable<T> LimitObjectResult<T>(IEnumerable<T> filteredResult, HttpRequest Request)
        {
            if (int.TryParse(Request.Query["offset"], out int offset))
                filteredResult = filteredResult.Skip(offset);

            if (int.TryParse(Request.Query["limit"], out int limit))
            {
                if (limit == 0)
                    return filteredResult;
                else
                    return filteredResult.Take(limit);
            }
            return filteredResult.Take(defaultLimit);
        }
    }
}
