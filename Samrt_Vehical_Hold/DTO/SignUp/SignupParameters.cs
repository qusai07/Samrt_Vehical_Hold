using Samrt_Vehical_Hold.Data;

namespace Samrt_Vehical_Hold.DTO.SignUp
{
    public class SignupParameters
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string NationalNumber { get; set; }
        public string Password { get; set; }
        public UserRole UserRole { get; set; }
    }
}
