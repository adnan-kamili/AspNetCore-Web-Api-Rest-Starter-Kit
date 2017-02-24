

namespace SampleApi.Models
{
    public abstract class BaseTenantEntity : BaseEntity, ITenantEntity
    {
        public string TenantId { get; set; }
    }
}