using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleApi.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public List<string> Claims { get; set; }
        public string NormalizedName
        {
            get
            {
                if (Name != null)
                {
                    return Name.ToUpper();
                }
                return null;
            }
        }
    }
}