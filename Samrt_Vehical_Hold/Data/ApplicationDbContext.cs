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
        public DbSet<Vehicle> vehicles { get; set; }
        public DbSet<HoldRequest> holdRequests { get; set; }


    }
}
