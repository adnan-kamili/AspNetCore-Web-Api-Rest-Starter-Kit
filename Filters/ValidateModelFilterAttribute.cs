using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SampleApi.Filters
{
    public class ValidateModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(context.ModelState);
                context.Result = new BadRequestObjectResult(modelErrors);
            }
        }
    }
}