using System.Security.Cryptography;

namespace Samrt_Vehical_Hold.Helpers.Service
{
    public static class OtpHelper
    {
        public static string GenerateOtp(int length = 4)
        {
            if (length <= 0) throw new ArgumentException("OTP length must be positive.");

            int maxValue = (int)Math.Pow(10, length) - 1;
            int minValue = (int)Math.Pow(10, length - 1);

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);

            int otpNumber = (int)(value % (maxValue - minValue + 1)) + minValue;
            return otpNumber.ToString();
        }
    }
}
