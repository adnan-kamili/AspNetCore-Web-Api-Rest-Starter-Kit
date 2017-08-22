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

        private DateTime? createdAt;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt
        {
            get { return createdAt ?? DateTime.UtcNow; }
            set { createdAt = value; }
        }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedAt { get; set; }

        public virtual ICollection<Role> Roles { get; } = new List<Role>();
    }
}
