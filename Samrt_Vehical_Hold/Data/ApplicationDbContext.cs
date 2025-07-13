using Microsoft.EntityFrameworkCore;
using Samrt_Vehical_Hold.Entities;
using Samrt_Vehical_Hold.Models;

namespace Samrt_Vehical_Hold.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
        {
        }
        public DbSet<ApplicationUser> Users { get; set; }

        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<HoldRequest> HoldRequests { get; set; }
        public DbSet<Violation> Violations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Violation>()
                .HasOne(v => v.HoldRequest)
                .WithMany()
                .HasForeignKey(v => v.HoldRequestId);

            builder.Entity<Vehicle>()
               .HasOne(v => v.OwnerUser)
               .WithMany(u => u.Vehicles)
               .HasForeignKey(v => v.OwnerUserId)
               .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
