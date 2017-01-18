using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SampleApi.Filters;
using System.Threading.Tasks;
using SampleApi.Repository;
using SampleApi.Models;
using SampleApi.ViewModels;


namespace SampleApi.Controllers
{
    public class AccountsController : BaseController<Account>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountsController(IRepository repository, UserManager<ApplicationUser> userManager) : base(repository)
        {
            this._userManager = userManager;
        }
        // bug override base method  not being overriden
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegistrationViewModel model)
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
            
            var account = new Account {
                Company = model.Company
            };
            repository.Create(account);
            await repository.SaveAsync();
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
            return Created($"/api/v1/accounts/{account.Id}", new { message = "Account was created successfully!" });
        }
    }
}
