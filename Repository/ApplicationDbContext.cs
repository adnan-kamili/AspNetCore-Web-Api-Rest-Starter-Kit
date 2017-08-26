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
            base.OnModelCreating(modelBuilder);
            // modelBuilder.Entity<UserRole>()
            //     .HasKey(t => new { t.UserId, t.RoleId });
            modelBuilder.Entity<Role>()
                .HasMany(e => e.Claims)
                .WithOne()
                .HasForeignKey(e => e.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Roles)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>().HasOne<User>(e => e.User)
                .WithOne().HasForeignKey<UserRole>(e => e.UserId);
            modelBuilder.Entity<UserRole>().HasOne<Role>(e => e.Role)
                .WithOne().HasForeignKey<UserRole>(e => e.RoleId);

            modelBuilder.Entity<Role>(model =>
            {
                model.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique(false);
                model.HasIndex(r => new { r.NormalizedName, r.TenantId }).HasName("TenantRoleNameIndex").IsUnique();
            });
        }
    }

}
