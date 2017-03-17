using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models.ViewModels
{
    public class UserPasswordViewModel
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}