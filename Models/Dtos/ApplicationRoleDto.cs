using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

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
            Name = role.Name.Replace(role.TenantId, string.Empty),
            Description = role.Description,
            Claims = role.Claims.Select(claim => claim.ClaimValue).ToList(),
            CreatedAt = role.CreatedAt,
            ModifiedAt = role.ModifiedAt
        };
    }
}
