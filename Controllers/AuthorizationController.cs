using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;

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
                var user = await _repository.GetUserManager().FindByNameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid."
                    });
                }

                // Ensure the user is allowed to sign in.
                if (!await _repository.GetSignInManager().CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The specified user is not allowed to sign in."
                    });
                }

                // Reject the token request if two-factor authentication has been enabled by the user.
                if (_repository.GetUserManager().SupportsUserTwoFactor && await _repository.GetUserManager().GetTwoFactorEnabledAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The specified user is not allowed to sign in."
                    });
                }

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
            else if (request.IsRefreshTokenGrantType()) {
                // Retrieve the claims principal stored in the refresh token.
                var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(
                    OpenIdConnectServerDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
                var user = await _repository.GetUserManager().GetUserAsync(info.Principal);
                if (user == null) {
                    return BadRequest(new OpenIdConnectResponse {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The refresh token is no longer valid."
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await _repository.GetSignInManager().CanSignInAsync(user)) {
                    return BadRequest(new OpenIdConnectResponse {
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

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user,  AuthenticationProperties properties = null)
        {
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
            var roles = await _repository.GetUserManager().GetRolesAsync(user);
            var permissionClaims = new List<Claim>();

            // Get all the roles and add them to the role claim
            foreach (var role in roles)
            {
                // Remove tenant id from roles to make them user friendly
                identity.AddClaim(ClaimTypes.Role, role.Substring(0,role.Length - user.TenantId.Length),
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);
                // Get all the permission claims of the role
                permissionClaims.AddRange(await _repository.GetRoleManager().GetClaimsAsync(new ApplicationRole(role, user.TenantId)));
            }
            // Get all the permission claims of the user if any

            // add check for permission claim - not needed access claims through roles only
            // permissionClaims.AddRange(await _repository.GetUserManager().GetClaimsAsync(user));

            identity.AddClaim(ClaimTypes.NameIdentifier, user.Id,
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);
            identity.AddClaim(ClaimTypes.Name, user.Email,
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

            if(user is ITenantEntity ){
                identity.AddClaim(CustomClaimTypes.Tid, user.TenantId,
                OpenIdConnectConstants.Destinations.AccessToken);
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
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
                scopes.Add(claim.Value);
            }
            ticket.SetScopes(scopes);
            ticket.SetResources(_appOptions.Jwt.Audience);
            return ticket;
        }
    }
}