using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using SampleApi.Repository;

namespace SampleApi.Controllers
{
    [Route("~/v1/[controller]")]
    public abstract class BaseController : Controller
    {
        protected readonly IRepository repository;

        protected const int minLimit = 10;

        protected const int maxLimit = 100;

        protected const int firstPage = 1;

        public BaseController(IRepository repository)
        {
            this.repository = repository;
        }
        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var modelErrors = new Dictionary<string, Object>();
            modelErrors["message"] = "The request has validation errors.";
            modelErrors["errors"] = new SerializableError(ModelState);
            return base.BadRequest(modelErrors);
        }
    }
}
