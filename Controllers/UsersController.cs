using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using SampleApi.Models;
using SampleApi.Repository;
using SampleApi.Models.Dtos;
using SampleApi.Policies;
using SampleApi.Filters;
using SampleApi.Models.ViewModels;

namespace SampleApi.Controllers
{
    [Route("~/v1/[controller]")]
    public class UsersController : BaseController
    {
        private string[] _includeProperties = { "Roles" };
        public UsersController(IRepository repository) : base(repository)
        {
        }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = PermissionClaims.ReadUsers)]
        public async Task<IActionResult> GetList([FromQuery] string[] roles, [FromQuery] int page = firstPage, [FromQuery] int limit = minLimit)
        {
            page = (page < firstPage) ? firstPage : page;
            limit = (limit < minLimit) ? minLimit : limit;
            limit = (limit > maxLimit) ? maxLimit : limit;
            int skip = (page - 1) * limit;
            Expression<Func<ApplicationUser, bool>> filter = null;
            if (roles.Length > 0)
            {
                var roleIds = new List<string>();
                foreach (string roleName in roles){
                    var role = await repository.GetRoleManager().FindByNameAsync(roleName + repository.TenantId);
                    if(role != null){
                        roleIds.Add(role.Id);
                    }
                }
                // get users by given roles
                filter = user => user.Roles.Select(role => role.RoleId).Any(roleId => roleIds.Contains(roleId));
            }
            int count = await repository.GetCountAsync<ApplicationUser>(filter);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = page.ToString();
            HttpContext.Items["limit"] = limit.ToString();
            var userList = await repository.GetAsync<ApplicationUser, ApplicationUserDto>(ApplicationUserDto.SelectProperties, filter, null, null, skip, limit);
            return Ok(userList);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PermissionClaims.ReadUser)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var user = await repository.GetByIdAsync<ApplicationUser, ApplicationUserDto>(id, ApplicationUserDto.SelectProperties, _includeProperties);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound(new { message = "User does not exist" });
        }

        [HttpPost]
        [Authorize(Policy = PermissionClaims.CreateUser)]
        public async Task<IActionResult> Create([FromBody] UserViewModel model)
        {
            if (await repository.GetUserManager().FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Email is already in use");
                return BadRequest(ModelState);
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                TenantId = repository.TenantId
            };
            foreach (var roleId in model.Roles.Distinct())
            {
                var role = await repository.GetByIdAsync<ApplicationRole>(roleId);
                if (role == null)
                {
                    ModelState.AddModelError("Role", $"Role '{roleId}' does not exist");
                    return BadRequest(ModelState);
                }
                else if (role.Name == "admin" + repository.TenantId)
                {
                    ModelState.AddModelError("Role", $"Role '{roleId}' is an admin role");
                    return BadRequest(ModelState);
                }
                user.Roles.Add(new IdentityUserRole<string>
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
            var result = await repository.GetUserManager().CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            await repository.SaveAsync();
            return Created($"/v1/users/{user.Id}", new { message = "User was created successfully!" });
        }

        [HttpPatch("{id}")] // needs update in update, partial role updates
        [Authorize(Policy = PermissionClaims.UpdateUser)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserViewModel model)
        {
            ApplicationUser user = await repository.GetByIdAsync<ApplicationUser>(id, _includeProperties);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist!" });
            }
            if (!HttpContext.User.IsInRole("admin"))
            {
                // only admin or current user can update current user's profile
                if (!HttpContext.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id))
                {
                    return Forbid();
                }
            }
            var rolesToAdd = new List<string>();
            var rolesToRemove = new List<string>();

            // only non-admin  user roles can be updated
            if (!HttpContext.User.IsInRole("admin"))
            {
                foreach (var roleId in model.Roles)
                {
                    var role = await repository.GetByIdAsync<ApplicationRole>(roleId);
                    if (role == null)
                    {
                        ModelState.AddModelError("Role", $"Role '{roleId}' does not exist");
                        return BadRequest(ModelState);
                    }
                    else if (role.Name == "admin" + repository.TenantId)
                    {
                        ModelState.AddModelError("Role", $"Role '{roleId}' is an admin role");
                        return BadRequest(ModelState);
                    }
                    if (user.Roles.Select(r => r.RoleId == roleId) != null)
                    {
                        // ensure stored role names are unique across tenants
                        rolesToAdd.Add(role.Name + repository.TenantId);
                    }

                }
                foreach (var userRole in user.Roles)
                {
                    if (model.Roles.Contains(userRole.RoleId) == false)
                    {
                        var role = await repository.GetByIdAsync<ApplicationRole>(userRole.RoleId);
                        // ensure stored role names are unique across tenants
                        rolesToRemove.Add(role.Name + repository.TenantId);
                    }
                }
            }

            if (!String.IsNullOrEmpty(model.Name))
            {
                user.Name = model.Name;
            }
            user.ModifiedAt = DateTime.UtcNow;
            var userUpdateResult = await repository.GetUserManager().UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                foreach (var error in userUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            await repository.GetUserManager().RemoveFromRolesAsync(user, rolesToRemove);
            await repository.GetUserManager().AddToRolesAsync(user, rolesToAdd);
            return NoContent();
        }

        [HttpPut("{id}/password")]
        [Authorize(Policy = PermissionClaims.UpdateUser)]
        public async Task<IActionResult> UpdatePassword([FromRoute] string id, [FromBody] UserPasswordViewModel model)
        {
            ApplicationUser user = await repository.GetByIdAsync<ApplicationUser>(id, _includeProperties);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist!" });
            }
            if (!HttpContext.User.IsInRole("admin"))
            {
                // only admin or current user can update current user's profile
                if (!HttpContext.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id))
                {
                    return Forbid();
                }
            }
            var result = await repository.GetUserManager().ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            user.ModifiedAt = DateTime.UtcNow;
            await repository.GetUserManager().UpdateAsync(user);
            return NoContent();
        }

        [HttpPut("{id}/email")]
        [Authorize(Policy = PermissionClaims.UpdateUser)]
        public async Task<IActionResult> UpdateEmail([FromRoute] string id, [FromBody] UserEmailViewModel model)
        {
            ApplicationUser user = await repository.GetByIdAsync<ApplicationUser>(id, _includeProperties);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist!" });
            }
            if (!HttpContext.User.IsInRole("admin"))
            {
                // only admin or current user can update current user's email
                if (!HttpContext.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id))
                {
                    return Forbid();
                }
            }
            if (model.Email != user.Email)
            {
                if (await repository.GetUserManager().FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("email", "Email is already in use");
                    return BadRequest(ModelState);
                }
            }
            user.Email = model.Email;
            user.ModifiedAt = DateTime.UtcNow;
            var result = await repository.GetUserManager().UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("email", error.Description);
                }
                return BadRequest(ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PermissionClaims.DeleteUser)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            ApplicationUser user = await repository.GetByIdAsync<ApplicationUser>(id);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist!" });
            }
            var adminRole = "admin" + repository.TenantId;
            if (await repository.GetUserManager().IsInRoleAsync(user, adminRole))
            {
                // admin user can't be deleted
                return Forbid();
            }
            repository.Delete(user);
            await repository.SaveAsync();
            return NoContent();
        }
    }


}
