using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech.Filters
{
    public class CacheMiddleware
    {
        //The RequestDelegate represents the next middleware in the pipeline.
        private RequestDelegate _next;

        //Said delegate must be passed in from the previous middleware.
        public CacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //The Invoke method is called by the previous middleware 
        //to kick off the current one.
        public Task Invoke(HttpContext context)
        {
            //custom static files middleware with cache
            if (Startup.cache)
            {
                //get file from cache and put into response
                var filename = Startup.basePath + context.Request.Path.Value.Replace("/", Path.DirectorySeparatorChar.ToString());
                if (Startup.staticFilesStorage.TryGetValue(filename, out var result))
                {
                    //var stream = new MemoryStream(result.Item2);
                    //var body = new StreamReader(stream).ReadToEnd();
                    //context.Response.ContentType = result.Item1;
                    //context.Response.ContentLength = body.Length;
                    return Task.Factory.StartNew(() => new FileContentResult(result.Item2, result.Item1));
                } 
            }
            //Finally, we call Invoke() on the next middleware in the pipeline.
            return _next.Invoke(context);
        }
    }
}
