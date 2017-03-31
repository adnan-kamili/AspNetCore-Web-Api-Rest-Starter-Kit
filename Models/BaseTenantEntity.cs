using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleApi.Models
{
    public abstract class BaseTenantEntity : BaseEntity, ITenantEntity
    {
        [Required]
        [Column(TypeName = "varchar(127)")]
        public string TenantId { get; set; }
    }
}