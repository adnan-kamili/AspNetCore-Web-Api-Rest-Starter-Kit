using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace SampleApi.Models
{
    public class ApplicationRole : IdentityRole, IEntity, ITenantEntity
    {
        public ApplicationRole()
        {
        }
        public ApplicationRole(string name, string tenantId)
            : base(name+tenantId)
        {
            this.TenantId = tenantId;
        }
        public ApplicationRole(string name, string tenantId, string description)
            : base(name+tenantId)
        {
            this.TenantId = tenantId;
            this.Description = description;
        }

        public string Description { get; set; }

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
    }
}
