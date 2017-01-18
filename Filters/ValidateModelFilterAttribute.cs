using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace SampleApi.Filters
{
    public class ValidateModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            if (!context.ModelState.IsValid && context.HttpContext.Request.Method == "PATCH")
            {
                foreach (var model in context.ModelState)
                {
                    //string[] parameterParts = parameter.Split('.');
                    //string argumentName = parameterParts[0];
                    //string propertyName = parameterParts[0];
                    Console.WriteLine(model.Value.Errors.ToString());
                    //Console.WriteLine(propertyName);
                    // var argument = context.ActionArguments[argumentName];
                    // var property = argument.GetType().GetProperty(propertyName);
                    // if (property.IsDefined(typeof(RequiredAttribute), true))
                    // {
                    //    Console.WriteLine(property);
                    // }
                }
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
            if (!context.ModelState.IsValid)
            {
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(context.ModelState);
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}