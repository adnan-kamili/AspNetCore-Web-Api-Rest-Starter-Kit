using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models.ViewModels
{
    public class UserViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        [Required]
        public List<string> Roles { get; set; }

    }
}