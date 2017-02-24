using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SampleApi.Models
{
    public abstract class BaseEntity : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

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