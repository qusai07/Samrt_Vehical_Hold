namespace Samrt_Vehical_Hold.Models
{
    public class ApplicationUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string NationalNumber { get; set; }
        public string PasswordHash { get; set; }
        public string OtpCode { get; set; }
        public DateTime? OtpDate { get; set; }
        public bool IsActive { get; set; }


    }
}
