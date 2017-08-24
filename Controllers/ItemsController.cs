using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.Dtos;
using SampleApi.ViewModels;
using SampleApi.Policies;
using SampleApi.Filters;

namespace SampleApi.Controllers
{
    [Route("~/v1/items")]
    public class ItemsController : BaseController
    {
        public ItemsController(IRepository repository, IMapper mapper) : base(repository, mapper)
        { }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = PermissionClaims.ReadItem)]
        public async Task<IActionResult> GetAll(PaginationViewModel pagination)
        {
            int count = await repository.GetCountAsync<Item>(null);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = pagination.Page.ToString();
            HttpContext.Items["limit"] = pagination.Limit.ToString();
            var entities = await repository.GetAllAsync<Item, ItemDto>(null, null, pagination.Skip, pagination.Limit);
            return Ok(entities);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PermissionClaims.ReadItem)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var entity = await repository.GetByIdAsync<Item, ItemDto>(id);
            if (entity != null)
            {
                return Ok(entity);
            }
            return NotFound(new { message = "Item does not exist!" });
        }

        [HttpPost]
        [Authorize(Policy = PermissionClaims.CreateItem)]
        public async Task<IActionResult> Create([FromBody] ItemViewModel viewModel)
        {
            Item item = mapper.Map<Item>(viewModel);
            repository.Create(item);
            await repository.SaveAsync();
            return Created($"/api/v1/items/{item.Id}", new { message = "Item was created successfully!" });
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = PermissionClaims.UpdateItem)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ItemViewModel updatedItem)
        {
            Item item = await repository.GetByIdAsync<Item>(id);
            if (item == null)
            {
                return NotFound(new { message = "Item does not exist!" });
            }
            repository.Update(item, updatedItem);
            await repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PermissionClaims.DeleteItem)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            Item entity = await repository.GetByIdAsync<Item>(id);
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
