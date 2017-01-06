using System;
using System.Collections.Generic;
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

        protected const int MinLimit = 10;

        protected const int MaxLimit = 100;

        protected const int FirstPage = 1;

        public BaseController(IRepository repository)
        {
            this.repository = repository;
        }

        [PaginationHeadersFilter]
        [HttpGet]
        public virtual async Task<IActionResult> GetList([FromQuery] int page = FirstPage, [FromQuery] int limit = MinLimit)
        {
            page = (page < FirstPage) ? FirstPage : page;
            limit = (limit < MinLimit) ? MinLimit : limit;
            limit = (limit > MaxLimit) ? MaxLimit : limit;
            int skip = (page - 1) * limit;
            int count =  await repository.GetCountAsync<TEntity>(null);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = page.ToString();
            HttpContext.Items["limit"] = limit.ToString();
            IEnumerable<TEntity> entityList = await repository.GetAllAsync<TEntity>(null, null, skip, limit);
            return Json(entityList);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get([FromRoute] int id)
        {
            TEntity entity = await repository.GetByIdAsync<TEntity>(id);
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
        public virtual async Task<IActionResult> Update([FromRoute] int id, [FromBody] TEntity updatedEntity)
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
        public virtual async Task<IActionResult> Delete([FromRoute] int id)
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
