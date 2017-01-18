using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleApi.Models
{
    public abstract class Entity<T> : IEntity<T>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public T Id { get; set; }

        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (T)Convert.ChangeType(value, typeof(T)); }
        }

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