using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;

using OpenIddict.Core;
using OpenIddict.Models;
using SampleApi.Repository;
using SampleApi.Options;
using SampleApi.Models;
using SampleApi.Policies;

namespace SampleApi.Controllers
{
    public class AuthorizationController : Controller
    {
        private string[] _includeProperties = { "Roles.Role.Claims" };
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;
        private readonly IRepository _repository;
        private readonly AppOptions _appOptions;

        public AuthorizationController(
            OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
            IRepository repository,
            IOptions<AppOptions> appOptions)
        {
            _applicationManager = applicationManager;
            _repository = repository;
            _appOptions = appOptions.Value;
        }
        [AllowAnonymous]
        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Token(OpenIdConnectRequest request)
        {
            if (request.IsPasswordGrantType())
            {
                var user = await _repository.GetOneAsync<User>(u => u.Email == request.Username, _includeProperties);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                // Ensure the user is allowed to sign in.
                // if (!await _repository.GetSignInManager().CanSignInAsync(user))
                // {
                //     return BadRequest(new OpenIdConnectResponse
                //     {
                //         Error = OpenIdConnectConstants.Errors.InvalidGrant,
                //         ErrorDescription = "The specified user is not allowed to sign in."
                //     });
                // }

                // Reject the token request if two-factor authentication has been enabled by the user.
                // if (_repository.GetUserManager().SupportsUserTwoFactor && await _repository.GetUserManager().GetTwoFactorEnabledAsync(user))
                // {
                //     return BadRequest(new OpenIdConnectResponse
                //     {
                //         Error = OpenIdConnectConstants.Errors.InvalidGrant,
                //         ErrorDescription = "The specified user is not allowed to sign in."
                //     });
                // }

                // Ensure the user is not already locked out.
                if (_repository.GetUserManager().SupportsUserLockout && await _repository.GetUserManager().IsLockedOutAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                // Ensure the password is valid.
                if (!await _repository.GetUserManager().CheckPasswordAsync(user, request.Password))
                {
                    if (_repository.GetUserManager().SupportsUserLockout)
                    {
                        await _repository.GetUserManager().AccessFailedAsync(user);
                    }
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                if (_repository.GetUserManager().SupportsUserLockout)
                {
                    await _repository.GetUserManager().ResetAccessFailedCountAsync(user);
                }

                // Create a new authentication ticket.
                var ticket = await CreateTicketAsync(request, user);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }
            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var info = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                var user = await _repository.GetUserManager().GetUserAsync(info.Principal);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The refresh token is no longer valid."
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await _repository.GetSignInManager().CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                // Create a new authentication ticket, but reuse the properties stored
                // in the refresh token, including the scopes originally granted.
                var ticket = await CreateTicketAsync(request, user, info.Properties);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        private async Task<Microsoft.AspNetCore.Authentication.AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, User user, AuthenticationProperties properties = null)
        {
            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationScheme,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role);
            //var roleNames = await _repository.GetUserManager().GetRolesAsync(user);
            var permissionClaims = new List<string>();

            // Get all the roles and add them to the role claim
            foreach (var userRole in user.Roles)
            {
                identity.AddClaim(OpenIdConnectConstants.Claims.Role, userRole.Role.Name,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);
                //var role = await _repository.GetRoleManager().FindByNameAsync(roleName);
                // Get all the permission claims of the role
                permissionClaims.AddRange(userRole.Role.Claims.Select(c => c.ClaimValue));//await _repository.GetRoleManager().GetClaimsAsync());
            }
            // Get all the permission claims of the user if any

            // add check for permission claim - not needed access claims through roles only
            // permissionClaims.AddRange(await _repository.GetUserManager().GetClaimsAsync(user));

            identity.AddClaim(OpenIdConnectConstants.Claims.Subject, user.Id,
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);
            identity.AddClaim(OpenIdConnectConstants.Claims.Email, user.Email,
                OpenIdConnectConstants.Destinations.IdentityToken);
            identity.AddClaim(OpenIdConnectConstants.Claims.Name, user.Name,
                OpenIdConnectConstants.Destinations.IdentityToken);

            if (user is ITenantEntity)
            {
                identity.AddClaim(CustomClaimTypes.TenantId, user.TenantId,
                OpenIdConnectConstants.Destinations.AccessToken);
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(
                new ClaimsPrincipal(identity), null,
                OpenIdConnectServerDefaults.AuthenticationScheme);

            // Set the list of scopes granted to the client application.
            // Note: the offline_access scope must be granted
            // to allow OpenIddict to return a refresh token.

            var scopes = new List<string> {
                OpenIdConnectConstants.Scopes.OpenId,
                OpenIdConnectConstants.Scopes.Email,
                OpenIdConnectConstants.Scopes.Profile,
                OpenIdConnectConstants.Scopes.OfflineAccess,
                OpenIddictConstants.Scopes.Roles
            }.Intersect(request.GetScopes()).ToList();
            // Add permission claims to scope
            foreach (var claim in permissionClaims)
            {
                scopes.Add(claim);
            }
            ticket.SetScopes(scopes);
            ticket.SetAudiences(_appOptions.Jwt.Audiences);
            ticket.SetAccessTokenLifetime(TimeSpan.FromSeconds(_appOptions.Jwt.AccessTokenLifetime));
            ticket.SetIdentityTokenLifetime(TimeSpan.FromSeconds(_appOptions.Jwt.IdentityTokenLifetime));
            ticket.SetRefreshTokenLifetime(TimeSpan.FromSeconds(_appOptions.Jwt.RefreshTokenLifetime));
            return ticket;
        }
    }
}