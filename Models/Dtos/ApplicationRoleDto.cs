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
            Name = role.Name.Substring(0, role.Name.Length - role.TenantId.Length),
            Description = role.Description,
            Claims = role.Claims.Select(claim => claim.ClaimValue).ToList(),
            CreatedAt = role.CreatedAt,
            ModifiedAt = role.ModifiedAt
        };
    }
}
