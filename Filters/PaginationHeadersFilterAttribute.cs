using Microsoft.AspNetCore.Mvc.Filters;

namespace SampleApi.Filters
{
    public class PaginationHeadersFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Add("X-Pagination-Count", context.HttpContext.Items["count"].ToString());
            context.HttpContext.Response.Headers.Add("X-Pagination-Page", context.HttpContext.Items["page"].ToString());
            context.HttpContext.Response.Headers.Add("X-Pagination-Limit", context.HttpContext.Items["limit"].ToString());
        }
    }
}