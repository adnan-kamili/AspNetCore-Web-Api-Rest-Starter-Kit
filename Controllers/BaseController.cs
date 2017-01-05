using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Filters;
using System.Threading.Tasks;
using SampleApi.Repository;
using SampleApi.Models;

namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
    [TypeFilter(typeof(CustomExceptionFilterAttribute))]
    [ValidateModelFilter]
    public abstract class BaseController<TEntity> : Controller where TEntity : Entity<int>
    {
        protected readonly IRepository repository;

        protected string EntityName = typeof(TEntity).Name;

        public BaseController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public virtual IActionResult GetList()
        {
            return Json(repository.GetAll<TEntity>());
        }

        [HttpGet("{id}")]
        public virtual IActionResult Get(int id)
        {
            TEntity entity = repository.GetById<TEntity>(id);
            if (entity != null)
            {
                return Json(entity);
            }
            return NotFound(new { message = EntityName + " does not exist!" });
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TEntity entity)
        {
            repository.Create(entity);
            await repository.SaveAsync();
            return Created($"/api/v1/{EntityName.ToLower()}s/{entity.Id}", new { message = $"{EntityName} was created successfully!" });
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> Update(int id, [FromBody] TEntity updatedEntity)
        {
            TEntity entity = repository.GetById<TEntity>(id);
            if (entity == null)
            {
                 return NotFound(new { message = $"{EntityName} does not exist!" });
            }
            repository.Update(entity, updatedEntity);
            await repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            TEntity entity = repository.GetById<TEntity>(id);
            if (entity == null)
            {
                 return NotFound(new { message = $"{EntityName} does not exist!" });
            }
            repository.Delete(entity);
            await repository.SaveAsync();
            return NoContent();
        }
    }
}
