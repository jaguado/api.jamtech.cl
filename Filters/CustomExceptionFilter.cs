using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JAMTech.Filters
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception != null)
                NewRelic.Api.Agent.NewRelic.NoticeError(context.Exception);
            return base.OnExceptionAsync(context);
        }
    }
}
