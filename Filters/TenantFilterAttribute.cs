using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

using SampleApi.Repository;
using SampleApi.Policies;

namespace SampleApi.Filters
{
    public class TenantFilterAttribute : ActionFilterAttribute
    {
        private IRepository _repository;
        public TenantFilterAttribute(IRepository repository)
        {
            _repository = repository;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Set tenant id in the repository
            ClaimsIdentity claimsIdentity = context.HttpContext.User.Identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.FindFirst(CustomClaimTypes.TenantId);
            if (claim != null)
            {
                _repository.TenantId = claim.Value;
            }

        }
    }
}