
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SampleApi.Repository;
using SampleApi.Models;


namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
    public abstract class BaseController<TEntity> : Controller
    {
        protected readonly IRepository repository;

        protected const int minLimit = 10;

        protected const int maxLimit = 100;

        protected const int firstPage = 1;

        public BaseController(IRepository repository)
        {
            this.repository = repository;
            if (typeof(ITenantEntity).GetTypeInfo().IsAssignableFrom(typeof(TEntity).Ge‌​tTypeInfo()))
            {
                ClaimsIdentity claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
                Claim claim = claimsIdentity?.FindFirst("tenant");
                repository.TenantId = claim.Value;
            }
        }
    }
}
