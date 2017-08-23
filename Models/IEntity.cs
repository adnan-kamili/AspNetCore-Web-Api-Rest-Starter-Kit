using System;

namespace  SampleApi.Models
{
    public interface IEntity
    {
        string Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }
}