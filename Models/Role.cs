using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using System.Collections.Generic;


namespace SampleApi.Models
{
    public class Role : IdentityRole, IEntity, ITenantEntity
    {
        public string Description { get; set; }

        [Required]
        public string TenantId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ModifiedAt { get; set; }

        public  ICollection<IdentityRoleClaim<string>> Claims { get; set;} = new List<IdentityRoleClaim<string>>();
    }
}
