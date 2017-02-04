namespace SampleApi.Models
{
    public abstract class TenantEntity<T> : Entity<T>, ITenantEntity
    {
        public string TenantId { get; set; }
    }
}