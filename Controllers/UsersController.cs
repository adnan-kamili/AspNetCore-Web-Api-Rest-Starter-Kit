using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

using SampleApi.Models;
using SampleApi.Repository;
using SampleApi.Dtos;
using SampleApi.Policies;
using SampleApi.Filters;
using SampleApi.ViewModels;

namespace SampleApi.Controllers
{
    [Route("~/v1/users")]
    public class UsersController : BaseController
    {
        private string[] _includeProperties = { "Roles.Role" };
        public UsersController(IRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = PermissionClaims.ReadUsers)]
        public async Task<IActionResult> GetAll(PaginationViewModel pagination)
        {
            int count = await repository.GetCountAsync<User>();
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = pagination.Page.ToString();
            HttpContext.Items["limit"] = pagination.Limit.ToString();
            var users = await repository.GetAllAsync<User, UserDto>(pagination.Skip, pagination.Limit, _includeProperties);
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PermissionClaims.ReadUser)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            UserDto user = await repository.GetByIdAsync<User, UserDto>(id, _includeProperties);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist" });
            }
            return Ok(user);
        }

        [HttpPost]
        [Authorize(Policy = PermissionClaims.CreateUser)]
        public async Task<IActionResult> Create([FromBody] UserViewModel viewModel)
        {
            if (await repository.GetUserManager().FindByEmailAsync(viewModel.Email) != null)
            {
                ModelState.AddModelError("Email", "Email is already in use");
                return BadRequest(ModelState);
            }
            var user = new User
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                Name = viewModel.Name,
                TenantId = repository.TenantId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            if (viewModel.Roles != null)
            {
                var distinctRoles = viewModel.Roles.Distinct().ToList();
                var roles = await repository.GetAsync<Role>();
                foreach (var roleName in distinctRoles)
                {
                    var role = roles.Where(r => r.NormalizedName == roleName.ToUpper()).SingleOrDefault();
                    if (role == null)
                    {
                        ModelState.AddModelError("Role", $"Role '{roleName}' does not exist");
                        return BadRequest(ModelState);
                    }
                    else if (role.NormalizedName == "ADMIN")
                    {
                        ModelState.AddModelError("Role", $"Role '{roleName}' is an admin role");
                        return BadRequest(ModelState);
                    }
                    user.Roles.Add(new UserRole()
                    {
                        RoleId = role.Id,
                        UserId = user.Id
                    });
                }
            }

            var result = await repository.GetUserManager().CreateAsync(user, viewModel.Password);
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

        [HttpPatch("{id}")]
        [Authorize(Policy = PermissionClaims.UpdateUser)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserViewModel viewModel)
        {
            User user = await repository.GetByIdAsync<User>(id, _includeProperties);
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
            // only non-admin  user roles can be updated
            if (viewModel.Roles != null && HttpContext.User.IsInRole("admin") &&  !user.Roles.Any(role => role.Role.Name == "admin"))
            {
                var distinctRoles = viewModel.Roles.Distinct().ToList();
                // remove existing roles not present in update request
                user.Roles = user.Roles.Where(r => distinctRoles.Contains(r.Role.Name)).ToList();
                var roles = await repository.GetAsync<Role>();
                foreach (var roleName in distinctRoles)
                {
                    var role = roles.Where(r => r.NormalizedName == roleName.ToUpper()).SingleOrDefault();
                    if (role == null)
                    {
                        ModelState.AddModelError("Role", $"Role '{roleName}' does not exist");
                        return BadRequest(ModelState);
                    }
                    else if (role.NormalizedName == "ADMIN")
                    {
                        ModelState.AddModelError("Role", $"Role '{roleName}' is an admin role");
                        return BadRequest(ModelState);
                    }
                    if (!user.Roles.Any(r => r.Role.Name == roleName))
                    {
                        user.Roles.Add(new UserRole()
                        {
                            RoleId = role.Id,
                            UserId = user.Id
                        });
                    }

                }
            }
            if (!String.IsNullOrEmpty(viewModel.Name))
            {
                user.Name = viewModel.Name;
            }
            await repository.GetUserManager().UpdateAsync(user);
            return NoContent();
        }

        [HttpPut("{id}/password")]
        [Authorize(Policy = PermissionClaims.UpdateUser)]
        public async Task<IActionResult> UpdatePassword([FromRoute] string id, [FromBody] UserPasswordViewModel viewModel)
        {
            User user = await repository.GetByIdAsync<User>(id, _includeProperties);
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
            var result = await repository.GetUserManager().ChangePasswordAsync(user, viewModel.Password, viewModel.NewPassword);
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
        public async Task<IActionResult> UpdateEmail([FromRoute] string id, [FromBody] UserEmailViewModel viewModel)
        {
            User user = await repository.GetByIdAsync<User>(id, _includeProperties);
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
            if (viewModel.Email != user.Email)
            {
                if (await repository.GetUserManager().FindByEmailAsync(viewModel.Email) != null)
                {
                    ModelState.AddModelError("email", "Email is already in use");
                    return BadRequest(ModelState);
                }
            }
            user.Email = viewModel.Email;
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
            User user = await repository.GetByIdAsync<User>(id, _includeProperties);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist!" });
            }

            // Admin user can't be deleted
            if (user.Roles.Any(role => role.Role.Name == "admin"))
            {
                return Forbid();
            }
            repository.Delete(user);
            await repository.SaveAsync();
            return NoContent();
        }
    }


}
