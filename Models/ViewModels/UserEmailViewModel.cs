using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models.ViewModels
{
    public class UserEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}