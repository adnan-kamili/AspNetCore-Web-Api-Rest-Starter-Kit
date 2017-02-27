using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace SampleApi.Models.Dtos
{
    public class ApplicationUserDto: BaseDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> Roles{ get; set; }
        
        public static Expression<Func<ApplicationUser, ApplicationUserDto>> SelectProperties = (user) => new ApplicationUserDto {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Roles = user.Roles.Select(role=> role.RoleId).ToList(),
            CreatedAt = user.CreatedAt,
            ModifiedAt = user.ModifiedAt
        };
    }
}
