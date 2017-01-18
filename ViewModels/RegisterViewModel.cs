using System.ComponentModel.DataAnnotations;
using SampleApi.Models;

namespace SampleApi.ViewModels
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