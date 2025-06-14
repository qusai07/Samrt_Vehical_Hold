using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Samrt_Vehical_Hold.Helpers.Service
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            var hashOfInput = HashPassword(enteredPassword);
            return hashOfInput == storedHashedPassword;
        }
    }
}
