using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models
{
    public class Book : Entity<int>
    {
        [Required]
        public string Name { get; set; }
        public string Author { get; set; }
        public string Price { get; set; }
        [Required]
        public int AccountId { get; set; }
    }
}
