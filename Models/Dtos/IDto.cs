using System;

namespace SampleApi.Models.Dtos
{
    public interface IDto
    {
        string Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
    }
}
