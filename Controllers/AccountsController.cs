using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;
using SampleApi.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SampleApi.Models;
using SampleApi.ViewModels;
using SampleApi.Filters;


namespace SampleApi.Controllers
{
    [Route("api/v1/[controller]")]
    [TypeFilter(typeof(CustomExceptionFilterAttribute))]
    [ValidateModelFilter]
    public class AccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository _repository;
        public AccountsController(
            IRepository repository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this._repository = repository;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
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
            var adminRole = await _roleManager.FindByNameAsync("admin");
            if (adminRole == null)
            {
                adminRole = new IdentityRole("admin");
                await _roleManager.CreateAsync(adminRole);
            }
            var newUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                AccountId = account.Id
            };
            var userCreationResult = await _userManager.CreateAsync(newUser, model.Password);
            if (!userCreationResult.Succeeded)
            {
                foreach (var error in userCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return BadRequest(ModelState);
            }
            await _userManager.AddToRoleAsync(newUser, adminRole.Name);
            return Created($"/api/v1/accounts/{account.Id}", new { message = "Account was created successfully!" });
        }
    }
}