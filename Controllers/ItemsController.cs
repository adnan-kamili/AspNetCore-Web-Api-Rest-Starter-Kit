using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.Policies;
using SampleApi.Filters;

namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class ItemsController : BaseController<Item>
    {
        public ItemsController(IRepository repository) : base(repository)
        {

        }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = Permissions.ReadItem)]
        public async Task<IActionResult> GetList([FromQuery] int page = FirstPage, [FromQuery] int limit = MinLimit)
        {
            page = (page < FirstPage) ? FirstPage : page;
            limit = (limit < MinLimit) ? MinLimit : limit;
            limit = (limit > MaxLimit) ? MaxLimit : limit;
            int skip = (page - 1) * limit;
            int count = await repository.GetCountAsync<Item>(null);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = page.ToString();
            HttpContext.Items["limit"] = limit.ToString();
            IEnumerable<Item> entityList = await repository.GetAllAsync<Item>(null, null, skip, limit);
            return Json(entityList);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.ReadItem)]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            Item entity = await repository.GetByIdAsync<Item>(id);
            if (entity != null)
            {
                return Json(entity);
            }
            return NotFound(new { message = "Item does not exist!" });
        }

        [HttpPost]
        [Authorize(Policy = Permissions.CreateItem)]
        public async Task<IActionResult> Create([FromBody] Item entity)
        {
            repository.Create(entity);
            await repository.SaveAsync();
            return Created($"/api/v1/items/{entity.Id}", new { message = "Item was created successfully!" });
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = Permissions.UpdateItem)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Item updatedEntity)
        {
            Item entity = repository.GetById<Item>(id);
            if (entity == null)
            {
                return NotFound(new { message = "Item does not exist!" });
            }
            repository.Update(entity, updatedEntity);
            await repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DeleteItem)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            Item entity = repository.GetById<Item>(id);
            if (entity == null)
            {
                return NotFound(new { message = "Item does not exist!" });
            }
            repository.Delete(entity);
            await repository.SaveAsync();
            return NoContent();
        }
    }


}
