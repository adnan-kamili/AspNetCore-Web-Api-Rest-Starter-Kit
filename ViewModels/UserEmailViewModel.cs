using System.ComponentModel.DataAnnotations;

namespace SampleApi.ViewModels
{
    public class UserEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}