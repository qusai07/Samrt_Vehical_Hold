using Microsoft.AspNetCore.Identity;
using Samrt_Vehical_Hold.Data;
using Samrt_Vehical_Hold.Entities;

namespace Samrt_Vehical_Hold.Models
{
    public class ApplicationUser
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string NationalNumber { get; set; }
        public string PasswordHash { get; set; }
        public string OtpCode { get; set; }
        public DateTime? OtpDate { get; set; }
        public bool IsActive { get; set; }
        public UserRole Role { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    }
}
