using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SampleApi.Repository;
using Microsoft.AspNetCore.Authorization;

using SampleApi.Models;
using SampleApi.ViewModels;
using SampleApi.Services;

namespace SampleApi.Controllers
{
    [Route("~/v1/accounts")]
    public class AccountsController : Controller
    {
        private readonly IRepository _repository;
        private readonly IEmailService _emailService;
        public AccountsController(IRepository repository, IEmailService emailService)
        {
            this._repository = repository;
            this._emailService = emailService;
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
            var user = new User
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
                _repository.Delete(tenant);
                await _repository.SaveAsync();
                return BadRequest(ModelState);
            }
            var adminRole = new Role("admin", tenant.Id, "Admin user with all the permissions");
            var roleCreationResult = await _repository.GetRoleManager().CreateAsync(adminRole);
            if (!roleCreationResult.Succeeded)
            {
                foreach (var error in roleCreationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                _repository.Delete(tenant);
                await _repository.GetUserManager().DeleteAsync(user);
                await _repository.SaveAsync();
                return BadRequest(ModelState);
            }
            await _repository.GetUserManager().AddToRoleAsync(user, adminRole.Name);
            return Created($"/api/v1/users/{user.Id}", new { message = "User account was created successfully!" });
        }

        [AllowAnonymous]
        [HttpPut("password")]
        public async Task<IActionResult> SendPasswordResetLink([FromBody] UserEmailViewModel model)
        {
            User user = await _repository.GetUserManager().FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = _repository.GetUserManager().GeneratePasswordResetTokenAsync(user).Result;
                var url = $"https://app.example.com/reset-password?email={model.Email}&token={token}";
                var message = $"<a href='{url}'>Click to reset password!</a>";
                await _emailService.SendEmailAsync(user.Email, "Password reset request", message);
            }
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPut("password-reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            User user = await _repository.GetUserManager().FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("email", "Email does not exist");
                return BadRequest(ModelState);
            }
            var result = _repository.GetUserManager().ResetPasswordAsync(user, model.Token, model.Password).Result;
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            return NoContent();
        }
    }
}
