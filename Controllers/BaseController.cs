using Microsoft.AspNetCore.Mvc;
using SampleApi.Repository;


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
        }
    }
}
