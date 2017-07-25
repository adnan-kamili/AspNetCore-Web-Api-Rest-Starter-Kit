using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SampleApi.Filters
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        public CustomExceptionFilterAttribute(IHostingEnvironment hostingEnvironment, ILogger<CustomExceptionFilterAttribute> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                string message = "Oops! Something is broken, we are looking into it";
                _logger.LogError(0, context.Exception, message);
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Result = new JsonResult(new { message = message });
            }
        }
    }
}