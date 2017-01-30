using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SampleApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SampleApi.Models;
using SampleApi.ViewModels;


namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
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
                ModelState.AddModelError("Email", "Email is already in use");
                var modelErrors = new Dictionary<string, Object>();
                modelErrors["message"] = "The request has validation errors.";
                modelErrors["errors"] = new SerializableError(ModelState);
                return BadRequest(ModelState);
            }

            var account = new Account
            {
                Company = model.Company
            };
            _repository.Create(account);
            await _repository.SaveAsync();
            var adminRole = new IdentityRole("admin");
            if (! await _repository.GetRoleManager().RoleExistsAsync(adminRole.Name))
            {
                await _repository.GetRoleManager().CreateAsync(adminRole);
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                AccountId = account.Id
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
            await _repository.GetUserManager().AddToRoleAsync(user, "admin");
            return Created($"/api/v1/accounts/{account.Id}", new { message = "Account was created successfully!" });
        }
    }
}