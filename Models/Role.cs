using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using System.Collections.Generic;


namespace SampleApi.Models
{
    public class Role : IdentityRole, IEntity, ITenantEntity
    {
        public Role()
        {
        }
        public Role(string name, string tenantId)
            : base(name+tenantId)
        {
            this.TenantId = tenantId;
        }
        public Role(string name, string tenantId, string description)
            : base(name+tenantId)
        {
            this.TenantId = tenantId;
            this.Description = description;
        }

        public string Description { get; set; }

        [Required]
        public string TenantId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ModifiedAt { get; set; }

        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set;}
    }
}
