using Microsoft.AspNetCore.Identity;

namespace Samrt_Vehical_Hold.Helpers
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

   
        public static string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
