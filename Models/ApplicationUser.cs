using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SampleApi.Models
{
    public class ApplicationUser : IdentityUser, IEntity<string>
    {
        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = value.ToString(); }
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public int AccountId { get; set; }

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
