namespace  SampleApi.Models
{
    public interface ITenantEntity : IEntity
    {
        string TenantId { get; set; }
    }
}