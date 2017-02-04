using System.ComponentModel.DataAnnotations;

namespace SampleApi.ViewModels
{
    public class UserViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RoleId { get; set; }
    }
}