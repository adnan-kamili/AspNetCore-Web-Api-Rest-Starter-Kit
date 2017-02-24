using System;
using System.Linq.Expressions;

namespace SampleApi.Models.Dtos
{
    public class ItemDto : BaseDto
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Color { get; set; }
        
        public static Expression<Func<Item, ItemDto>> SelectProperties = (item) => new ItemDto {
            Id = item.Id,
            Name = item.Name,
            Cost = item.Cost,
            Color = item.Color
        };
    }
}
