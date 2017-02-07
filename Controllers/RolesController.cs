using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.Policies;
using SampleApi.Filters;
using SampleApi.ViewModels;

namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class RolesController : BaseController<ApplicationRole>
    {
        public RolesController(IRepository repository) : base(repository)
        {
        }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = PermissionClaims.ReadRole)]
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
            IEnumerable<ApplicationRole> roleList = await repository.GetAllAsync<ApplicationRole>(null, null, skip, limit);
            return Json(roleList);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PermissionClaims.ReadRole)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            ApplicationRole role = await repository.GetByIdAsync<ApplicationRole>(id);
            if (role != null)
            {
                return Json(role);
            }
            return NotFound(new { message = "Role does not exist!" });
        }

        [HttpPost]
        [Authorize(Policy = PermissionClaims.CreateRole)]
        public async Task<IActionResult> Create([FromBody] RoleViewModel model)
        {
            if (await repository.GetRoleManager().RoleExistsAsync(model.Name + repository.TenantId))
            {
                ModelState.AddModelError("Role", $"Role {model.Name} already exists.");
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(ModelState);
                return BadRequest(modelErrors);
            }
            foreach (var claim in model.Claims)
            {
                if (!PermissionClaims.GetAll().Contains(claim))
                {
                    ModelState.AddModelError("RoleClaim", $"RoleClaim {claim} does not exist.");
                    var modelErrors = new Dictionary<string, Object>();
                    modelErrors["message"] = "The request has validation errors.";
                    modelErrors["errors"] = new SerializableError(ModelState);
                    return BadRequest(modelErrors);
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
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(ModelState);
                return BadRequest(modelErrors);
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
            ApplicationUser user = repository.GetById<ApplicationUser>(id);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist!" });
            }
            if (!HttpContext.User.IsInRole("admin" + repository.TenantId))
            {
                // only admin or current user can update current user's profile
                if (!HttpContext.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id))
                {
                    return Forbid();
                }
            }

            if (!String.IsNullOrEmpty(model.Name))
            {
                user.Name = model.Name;
            }
            if (!String.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
            }

            if (!String.IsNullOrEmpty(model.Password) && !String.IsNullOrEmpty(model.NewPassword))
            {
                var passwordResetResult = await repository.GetUserManager().ChangePasswordAsync(user, model.Password, model.NewPassword);
                if (!passwordResetResult.Succeeded)
                {
                    foreach (var error in passwordResetResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return BadRequest(ModelState);
                }
            }
            var userUpdateResult = await repository.GetUserManager().UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                foreach (var error in userUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PermissionClaims.DeleteRole)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            ApplicationRole role = repository.GetById<ApplicationRole>(id);
            if (await repository.GetRoleManager().FindByIdAsync(id) == null)
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
