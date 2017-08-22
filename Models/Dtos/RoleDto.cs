using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace SampleApi.Models.Dtos
{
    public class RoleDto : BaseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Claims { get; set; }

        public static Expression<Func<Role, RoleDto>> SelectProperties = (role) => new RoleDto
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
