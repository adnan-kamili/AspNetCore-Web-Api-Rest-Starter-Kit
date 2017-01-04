using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models
{
    public class Item : Entity<int>
    {
        [Required]
        public string Name { get; set; }
        public string Cost { get; set; }
        public string Color { get; set; }
    }
}
