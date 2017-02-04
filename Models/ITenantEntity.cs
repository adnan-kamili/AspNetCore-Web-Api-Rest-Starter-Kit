namespace SampleApi.Models
{
    public interface ITenantEntity : IEntity
    {
        object TenantId { get; set; }
    }
    public interface ITenantEntity<T> : ITenantEntity
    {
        new T TenantId { get; set; }
    }
}