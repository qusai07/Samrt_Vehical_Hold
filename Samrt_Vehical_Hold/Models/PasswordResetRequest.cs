namespace Samrt_Vehical_Hold.Models
{
    public class PasswordResetRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string ResetCode { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;

        public ApplicationUser User { get; set; } = null!;
    }

}
