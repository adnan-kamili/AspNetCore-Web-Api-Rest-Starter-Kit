namespace SampleApi.Models
{
    public class Tenant : Entity<string>
    {
        public string Company { get; set; }
    }
}