using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

using AutoMapper;

using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.Policies;
using SampleApi.Filters;
using SampleApi.ViewModels;
using SampleApi.Dtos;

namespace SampleApi.Controllers
{
    [Route("~/v1/roles")]
    public class RolesController : BaseController
    {
        private string[] _includeProperties = { "Claims" };
        public RolesController(IRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }

        [PaginationHeadersFilter]
        [HttpGet]
        [Authorize(Policy = PermissionClaims.ReadRoles)]
        public async Task<IActionResult> GetAll(PaginationViewModel pagination)
        {
            int count = await repository.GetCountAsync<Role>(null);
            HttpContext.Items["count"] = count.ToString();
            HttpContext.Items["page"] = pagination.Page.ToString();
            HttpContext.Items["limit"] = pagination.Limit.ToString();
            var roles = await repository.GetAllAsync<Role, RoleDto>(pagination.Skip, pagination.Limit, _includeProperties);
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PermissionClaims.ReadRole)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var role = await repository.GetByIdAsync<Role, RoleDto>(id, _includeProperties);
            if (role == null)
            {
                return NotFound(new { message = "Role with id '" + id + "' does not exist!" });
            }
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = PermissionClaims.CreateRole)]
        public async Task<IActionResult> Create([FromBody] RoleViewModel viewModel)
        {
            Expression<Func<Role, bool>> filter = existingRole => existingRole.NormalizedName == viewModel.NormalizedName;
            bool roleExists = await repository.AnyAsync<Role>(filter);
            if (roleExists)
            {
                ModelState.AddModelError("Role", $"Role {viewModel.Name} already exists.");
                return BadRequest(ModelState);
            }
            var role = mapper.Map<Role>(viewModel);
            role.NormalizedName = viewModel.NormalizedName;
            if (viewModel.Claims != null)
            {
                foreach (var claim in viewModel.Claims)
                {
                    if (!PermissionClaims.GetAll().Contains(claim))
                    {
                        ModelState.AddModelError("Claims", $"Claim {claim} does not exist.");
                        return BadRequest(ModelState);
                    }
                    role.Claims.Add(new IdentityRoleClaim<string>(){
                        RoleId = role.Id,
                        ClaimType = CustomClaimTypes.Permission,
                        ClaimValue = claim
                    });
                }
            }
            repository.Create(role);
            await repository.SaveAsync();
            return Created($"/api/v1/roles/{role.Id}", new { message = "Role was created successfully!" });
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = PermissionClaims.UpdateRole)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleViewModel viewModel)
        {
            Role role = await repository.GetByIdAsync<Role>(id, _includeProperties);
            if (role == null)
            {
                return NotFound(new { message = "Role does not exist!" });
            }
            if (role.NormalizedName == "ADMIN")
            {
                return Forbid();
            }
            if (viewModel.NormalizedName != role.NormalizedName)
            {
                Expression<Func<Role, bool>> filter = existingRole => existingRole.NormalizedName == viewModel.NormalizedName;
                bool roleExists = await repository.AnyAsync<Role>(filter);
                if (roleExists)
                {
                    ModelState.AddModelError("Role", $"Role {viewModel.Name} already exists.");
                    return BadRequest(ModelState);
                }
            }

            if (!String.IsNullOrEmpty(role.Name))
            {
                role.Name = viewModel.Name;
                role.NormalizedName = viewModel.NormalizedName;
            }
            if (!String.IsNullOrEmpty(viewModel.Description))
            {
                role.Description = viewModel.Description;
            }
            if (viewModel.Claims != null)
            {
                role.Claims.Clear();
                var distinctClaims = viewModel.Claims.Distinct().ToList();
                foreach (var claim in distinctClaims)
                {
                    if (!PermissionClaims.GetAll().Contains(claim))
                    {
                        ModelState.AddModelError("Claims", $"Claim {claim} does not exist.");
                        return BadRequest(ModelState);
                    }
                    role.Claims.Add(new IdentityRoleClaim<string>(){
                        RoleId = role.Id,
                        ClaimType = CustomClaimTypes.Permission,
                        ClaimValue = claim
                    });
                }
                repository.Delete<IdentityRoleClaim<string>>(claim=> claim.RoleId == role.Id);
            }
            repository.Update(role);
            await repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PermissionClaims.DeleteRole)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            Role role = await repository.GetByIdAsync<Role>(id);
            if (role == null)
            {
                return NotFound(new { message = "Role does not exist!" });
            }
            if (role.NormalizedName == "ADMIN")
            {
                return Forbid();
            }
            Expression<Func<User, bool>> filter = user => user.Roles.Select(r => r.RoleId).Any(roleId => roleId == role.Id);
            if (await repository.AnyAsync<User>(filter))
            {
                this.HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                return Json(new { message = "Role is in use!" });
            }
            repository.Delete(role);
            await repository.SaveAsync();
            return NoContent();
        }
    }


}
