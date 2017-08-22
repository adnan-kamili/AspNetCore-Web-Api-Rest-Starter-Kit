using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace SampleApi.Models.Dtos
{
    public class UserDto: BaseDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> Roles{ get; set; }
        
        public static Expression<Func<User, UserDto>> SelectProperties = (user) => new UserDto {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Roles = user.Roles.Select(role=> role.RoleId).ToList(),
            CreatedAt = user.CreatedAt,
            ModifiedAt = user.ModifiedAt
        };
    }
}
