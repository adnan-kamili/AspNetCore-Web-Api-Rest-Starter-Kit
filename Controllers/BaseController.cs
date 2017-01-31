using Microsoft.AspNetCore.Mvc;

using SampleApi.Repository;
using SampleApi.Models;


namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
    public abstract class BaseController<TEntity> : Controller where TEntity : Entity<int>
    {
        protected readonly IRepository repository;

        protected const int MinLimit = 10;

        protected const int MaxLimit = 100;

        protected const int FirstPage = 1;

        public BaseController(IRepository repository)
        {
            this.repository = repository;
        }
    }
}
