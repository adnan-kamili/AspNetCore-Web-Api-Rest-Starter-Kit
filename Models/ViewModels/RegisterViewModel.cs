using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string Company { get; set; }
    }
}