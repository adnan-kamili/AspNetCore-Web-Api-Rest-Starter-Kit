using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SampleApi.Models.Dtos
{
    public class ApplicationRoleDto : BaseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Claims { get; set; }

        public static Expression<Func<ApplicationRole, ApplicationRoleDto>> SelectProperties = (role) => new ApplicationRoleDto
        {
            Id = role.Id,
            Name = role.Name.Substring(0, role.Name.Length - role.TenantId.Length),
            Description = role.Description,
            //Claims = GetClaimsList(role.Claims),
            CreatedAt = role.CreatedAt,
            ModifiedAt = role.ModifiedAt
        };

        private static List<string> GetClaimsList(ICollection<IdentityRoleClaim<string>> roleClaims)
        {
            var claims = new List<string>();
            // foreach (var roleClaim in roleClaims)
            // {
            //     claims.Add(roleClaim.ClaimValue);
            // }
            return claims;
        }
    }
}
