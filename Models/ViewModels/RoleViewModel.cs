using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public List<string> Claims { get; set; } = new List<string>();
    }
}