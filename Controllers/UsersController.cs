using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Policy = PermissionClaims.ReadUser)]
        public async Task<IActionResult> GetList([FromQuery] int page = firstPage, [FromQuery] int limit = minLimit)
        {
            page = (page < firstPage) ? firstPage : page;
            limit = (limit < minLimit) ? minLimit : limit;
            limit = (limit > maxLimit) ? maxLimit : limit;
            int skip = (page - 1) * limit;
            int count = await repository.GetCountAsync<ApplicationUser>(null);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = page.ToString();
            HttpContext.Items["limit"] = limit.ToString();
            var userList = await repository.GetAllAsync<ApplicationUser, ApplicationUserDto>(ApplicationUserDto.SelectProperties, null, null, skip, limit);
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
            return NotFound(new { message = "User does not exist!" });
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
            for (var i = 0; i < model.Roles.Count; i++)
            {
                bool roleExists = await repository.GetRoleManager().RoleExistsAsync(model.Roles[i] + repository.TenantId);
                if (!roleExists || model.Roles[i] == "admin")
                {
                    ModelState.AddModelError("Role", $"Role '{model.Roles[i]}' does not exist");
                    return BadRequest(ModelState);
                }
                model.Roles[i] = model.Roles[i] + repository.TenantId;
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                TenantId = repository.TenantId
            };
            var userCreationResult = await repository.GetUserManager().CreateAsync(user, model.Password);
            if (!userCreationResult.Succeeded)
            {
                foreach (var error in userCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            await repository.GetUserManager().AddToRolesAsync(user, model.Roles);
            await repository.SaveAsync();
            return Created($"/api/v1/users/{user.Id}", new { message = "User was created successfully!" });
        }

        [HttpPatch("{id}")] // needs update in update, partial role updates
        [Authorize(Policy = PermissionClaims.UpdateUser)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserViewModel model)
        {
            ApplicationUser user = await repository.GetByIdAsync<ApplicationUser>(id);
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
            user.ModifiedAt = DateTime.UtcNow;
            // update password using existing password
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
