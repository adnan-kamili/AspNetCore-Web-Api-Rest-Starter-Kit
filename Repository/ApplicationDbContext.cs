using Microsoft.EntityFrameworkCore;
using SampleApi.Models;

namespace SampleApi.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public DbSet<Item> Items { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}
