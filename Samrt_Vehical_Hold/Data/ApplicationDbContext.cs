using Microsoft.EntityFrameworkCore;
using Samrt_Vehical_Hold.Models;
using System.Collections.Generic;

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

    }
}
