using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;


namespace SampleApi.Filters
{
    public class ValidateModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Allow partial update
            if (!context.ModelState.IsValid && context.HttpContext.Request.Method == "PATCH")
            {
                // improve code to remove check on hard coded string - "required"
                var modelStateErrors = context.ModelState.Where(model =>
                {
                    // ignore only if required error is present
                    if (model.Value.Errors.Count == 1)
                    {
                        Console.WriteLine(model.Value.Errors.FirstOrDefault().Exception.ToString());
                        // assuming required validation error message contains word "required"
                        return model.Value.Errors.FirstOrDefault().ErrorMessage.Contains("required");
                    }
                    return false;
                });
                foreach (var errorModel in modelStateErrors)
                {
                    context.ModelState.Remove(errorModel.Key.ToString());
                }

            }
            // Return validation error response
            else if (!context.ModelState.IsValid)
            {
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(context.ModelState);
                context.Result = new BadRequestObjectResult(context.ModelState);
            }

        }
    }
}