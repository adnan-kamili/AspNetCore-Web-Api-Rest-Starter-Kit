using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace SampleApi.Models
{
    public class User : IdentityUser, IEntity, ITenantEntity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string TenantId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ModifiedAt { get; set; }

        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
