using System.ComponentModel.DataAnnotations;

namespace SampleApi.Models
{
    public class Account : Entity<int>
    {
        public string Company { get; set; }
    }
}