using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SampleApi.Repository;
using Microsoft.AspNetCore.Authorization;

using SampleApi.Models;
using SampleApi.Models.ViewModels;


namespace SampleApi.Controllers
{
    [Route("~/v1/[controller]")]
    public class AccountsController : Controller
    {
        private readonly IRepository _repository;
        public AccountsController(IRepository repository)
        {
            this._repository = repository;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterViewModel model)
        {
            if (await _repository.GetUserManager().FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("email", "Email is already in use");
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(ModelState);
                return BadRequest(ModelState);
            }

            var tenant = new Tenant
            {
                Company = model.Company
            };
            _repository.Create(tenant);
            await _repository.SaveAsync();
            var adminRole = new ApplicationRole("admin", tenant.Id, "Admin user with all the permissions");
            var roleCreationResult = await _repository.GetRoleManager().CreateAsync(adminRole);
            if (!roleCreationResult.Succeeded)
            {
                foreach (var error in roleCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                TenantId = tenant.Id
            };
            var userCreationResult = await _repository.GetUserManager().CreateAsync(user, model.Password);
            if (!userCreationResult.Succeeded)
            {
                foreach (var error in userCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            await _repository.GetUserManager().AddToRoleAsync(user, adminRole.Name);
            return Created($"/api/v1/users/{user.Id}", new { message = "User account was created successfully!" });
        }
    }
}