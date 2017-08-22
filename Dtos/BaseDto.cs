using System;

namespace SampleApi.Dtos
{
    public abstract class BaseDto
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
