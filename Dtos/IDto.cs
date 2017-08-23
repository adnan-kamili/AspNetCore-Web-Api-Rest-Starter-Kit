using System;

namespace SampleApi.Dtos
{
    public interface IDto
    {
        string Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }
}
