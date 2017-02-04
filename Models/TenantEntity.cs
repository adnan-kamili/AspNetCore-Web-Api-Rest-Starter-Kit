using System;

namespace SampleApi.Models
{
    public abstract class TenantEntity<T> : Entity<T>, ITenantEntity<T>
    {
        public T TenantId { get; set; }

        object ITenantEntity.TenantId
        {
            get { return this.TenantId; }
            set { this.TenantId = (T)Convert.ChangeType(value, typeof(T)); }
        }
    }
}