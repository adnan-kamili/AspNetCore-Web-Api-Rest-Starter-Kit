using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SampleApi.Models;

namespace SampleApi.Repository
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public DbSet<Item> Items { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<ActivityScenario>()
            //     .HasKey(t => new { t.ActivityId, t.ScenarioId });

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>(model => {
                model.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique(false);
                model.HasIndex(r => new { r.NormalizedName, r.TenantId }).HasName("TenantRoleNameIndex").IsUnique();
            });
        }
    }

}
