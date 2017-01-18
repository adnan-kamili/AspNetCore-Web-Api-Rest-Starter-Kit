using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models
{
    public class Item : Entity<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Cost { get; set; }
        public string Color { get; set; }
        [Required]
        public int AccountId { get; set; }
    }
}
