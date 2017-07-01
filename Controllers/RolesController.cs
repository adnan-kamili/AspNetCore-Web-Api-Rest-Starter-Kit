using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;

using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.Policies;
using SampleApi.Filters;
using SampleApi.Models.ViewModels;
using SampleApi.Models.Dtos;

namespace SampleApi.Controllers
{
    [Route("~/v1/roles")]
    public class RolesController : BaseController
    {
        private string[] _includeProperties = { "Claims" };
        public RolesController(IRepository repository) : base(repository)
        {
        }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = PermissionClaims.ReadRoles)]
        public async Task<IActionResult> GetList([FromQuery] int page = firstPage, [FromQuery] int limit = minLimit)
        {
            page = (page < firstPage) ? firstPage : page;
            limit = (limit < minLimit) ? minLimit : limit;
            limit = (limit > maxLimit) ? maxLimit : limit;
            int skip = (page - 1) * limit;
            int count = await repository.GetCountAsync<ApplicationRole>(null);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = page.ToString();
            HttpContext.Items["limit"] = limit.ToString();
            var roleList = await repository.GetAllAsync<ApplicationRole, ApplicationRoleDto>(ApplicationRoleDto.SelectProperties, null, _includeProperties, skip, limit);
            return Ok(roleList);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PermissionClaims.ReadRole)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var role = await repository.GetByIdAsync<ApplicationRole, ApplicationRoleDto>(id, ApplicationRoleDto.SelectProperties, _includeProperties);
            if (role == null)
            {
                return NotFound(new { message = "Role with id '" + id + "' does not exist!" });
            }
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = PermissionClaims.CreateRole)]
        public async Task<IActionResult> Create([FromBody] RoleViewModel model)
        {
            // make role names case insensitive
            model.Name = model.Name.ToLower();
            if (await repository.GetRoleManager().RoleExistsAsync(model.Name + repository.TenantId))
            {
                ModelState.AddModelError("Role", $"Role {model.Name} already exists.");
                return BadRequest(ModelState);
            }
            if (model.Claims != null)
            {
                foreach (var claim in model.Claims)
                {
                    if (!PermissionClaims.GetAll().Contains(claim))
                    {
                        ModelState.AddModelError("Claims", $"Claim {claim} does not exist.");
                        return BadRequest(ModelState);
                    }
                }
            }
            var role = new ApplicationRole(model.Name, repository.TenantId, model.Description);
            var roleCreationResult = await repository.GetRoleManager().CreateAsync(role);
            if (!roleCreationResult.Succeeded)
            {
                foreach (var error in roleCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            if (model.Claims != null)
            {
                foreach (var roleClaim in model.Claims)
                {
                    await repository.GetRoleManager().AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, roleClaim));
                }
            }
            return Created($"/api/v1/roles/{role.Id}", new { message = "Role was created successfully!" });
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = PermissionClaims.UpdateRole)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleViewModel model)
        {
            ApplicationRole role = await repository.GetByIdAsync<ApplicationRole>(id);
            if (role == null)
            {
                return NotFound(new { message = "Role does not exist!" });
            }
            if (role.Name == "admin")
            {
                // admin role can't be updated
                return Forbid();
            }
            model.Name = model.Name.ToLower();
            if (model.Name + repository.TenantId != role.Name)
            {
                if (await repository.GetRoleManager().RoleExistsAsync(model.Name + repository.TenantId))
                {
                    ModelState.AddModelError("Role", $"Role {model.Name} already exists.");
                    return BadRequest(ModelState);
                }
            }
            if (model.Claims != null)
            {
                foreach (var claim in model.Claims)
                {
                    if (!PermissionClaims.GetAll().Contains(claim))
                    {
                        ModelState.AddModelError("Claims", $"Claim {claim} does not exist.");
                        return BadRequest(ModelState);
                    }
                }
            }

            if (!String.IsNullOrEmpty(role.Name))
            {
                role.Name = model.Name + repository.TenantId;
            }
            if (!String.IsNullOrEmpty(model.Description))
            {
                role.Description = model.Description;
            }
            role.ModifiedAt = DateTime.UtcNow;
            var roleUpdateResult = await repository.GetRoleManager().UpdateAsync(role);
            if (!roleUpdateResult.Succeeded)
            {
                foreach (var error in roleUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            if (model.Claims != null)
            {
                var claimsToRemove = await repository.GetRoleManager().GetClaimsAsync(role);
                foreach (var roleClaim in claimsToRemove)
                {
                    await repository.GetRoleManager().RemoveClaimAsync(role, roleClaim);
                }
                foreach (var roleClaim in model.Claims)
                {
                    await repository.GetRoleManager().AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, roleClaim));
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PermissionClaims.DeleteRole)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            ApplicationRole role = await repository.GetByIdAsync<ApplicationRole>(id);
            if (role == null)
            {
                return NotFound(new { message = "Role does not exist!" });
            }
            if (role.Name == "admin")
            {
                // admin role can't be deleted
                return Forbid();
            }
            Expression<Func<ApplicationUser, bool>> filter = user => user.Roles.Select(r => r.RoleId).Any(roleId => roleId == role.Id);
            var userList = await repository.GetAsync<ApplicationUser, ApplicationUserDto>(ApplicationUserDto.SelectProperties, filter, null, null);
            if (userList.ToList().Count > 0)
            {
                this.HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                return Json(new { message = "Role is in use!" });
            }
            var roleDeleteResult = repository.GetRoleManager().DeleteAsync(role).Result;
            if (!roleDeleteResult.Succeeded)
            {
                foreach (var error in roleDeleteResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            return NoContent();
        }
    }


}
