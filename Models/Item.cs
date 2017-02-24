using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models
{
    public class Item : BaseTenantEntity
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Cost { get; set; }
        public string Color { get; set; }
    }
}
