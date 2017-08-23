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

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ModifiedAt { get; set; }
    }
}