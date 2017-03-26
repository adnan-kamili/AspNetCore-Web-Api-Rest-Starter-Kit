using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.Policies;
using SampleApi.Filters;
using SampleApi.Models.ViewModels;
using SampleApi.Models.Dtos;

namespace SampleApi.Controllers
{
    [Route("~/v1/[controller]")]
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
            foreach (var claim in model.Claims)
            {
                if (!PermissionClaims.GetAll().Contains(claim))
                {
                    ModelState.AddModelError("Claims", $"Claim {claim} does not exist.");
                    return BadRequest(ModelState);
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
            foreach (var roleClaim in model.Claims)
            {
                await repository.GetRoleManager().AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, roleClaim));
            }

            return Created($"/api/v1/roles/{role.Id}", new { message = "Role was created successfully!" });
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = PermissionClaims.UpdateRole)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserViewModel model)
        {
            ApplicationRole role = await repository.GetByIdAsync<ApplicationRole>(id);
            if (role == null)
            {
                return NotFound(new { message = "Role does not exist!" });
            }
            role.ModifiedAt = DateTime.UtcNow;
            // // use repository get to validate tenancy
            // if (await repository.GetByIdAsync<ApplicationRole, ApplicationRoleDto>(id, ApplicationRoleDto.SelectProperties) == null)
            // {
            //     return NotFound(new { message = "Role does not exist!" });
            // }
            // // role
            // ApplicationUser user = repository.GetById<ApplicationUser>(id);
            // if (user == null)
            // {
            //     return NotFound(new { message = "User does not exist!" });
            // }
            // if (!HttpContext.User.IsInRole("admin" + repository.TenantId))
            // {
            //     // only admin or current user can update current user's profile
            //     if (!HttpContext.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id))
            //     {
            //         return Forbid();
            //     }
            // }

            // if (!String.IsNullOrEmpty(model.Name))
            // {
            //     user.Name = model.Name;
            // }
            // if (!String.IsNullOrEmpty(model.Email))
            // {
            //     user.Email = model.Email;
            // }

            // if (!String.IsNullOrEmpty(model.Password) && !String.IsNullOrEmpty(model.NewPassword))
            // {
            //     var passwordResetResult = await repository.GetUserManager().ChangePasswordAsync(user, model.Password, model.NewPassword);
            //     if (!passwordResetResult.Succeeded)
            //     {
            //         foreach (var error in passwordResetResult.Errors)
            //         {
            //             ModelState.AddModelError(string.Empty, error.Description);
            //         }

            //         return BadRequest(ModelState);
            //     }
            // }
            // var userUpdateResult = await repository.GetUserManager().UpdateAsync(user);
            // if (!userUpdateResult.Succeeded)
            // {
            //     foreach (var error in userUpdateResult.Errors)
            //     {
            //         ModelState.AddModelError(string.Empty, error.Description);
            //     }

            //     return BadRequest(ModelState);
            // }
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
