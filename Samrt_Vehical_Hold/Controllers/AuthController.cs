using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Samrt_Vehical_Hold.Data;
using Samrt_Vehical_Hold.DTO;
using Samrt_Vehical_Hold.Helpers;
using Samrt_Vehical_Hold.Models;


namespace Samrt_Vehical_Hold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;


        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtHelper = new JwtHelper(configuration);
        }


        [HttpPost("SignUp")]
        public IActionResult SignUp([FromBody] SignupParameters signupParameters) 
        {
            var errors = new List<string>();

            if (_context.Users.Any(x => x.UserName == signupParameters.UserName))
                errors.Add("UserNameUsed");

            if(_context.Users.Any(x => x.MobileNumber == signupParameters.MobileNumber))
                errors.Add("MobileNumberUsed");

            if (_context.Users.Any(x => x.EmailAddress == signupParameters.EmailAddress))
                errors.Add("EmailAddressUsed");
            
            if(errors.Any())
                return BadRequest(errors);
            var user = new ApplicationUser
            {
                FullName = signupParameters.FullName,
                UserName = signupParameters.UserName,
                EmailAddress = signupParameters.EmailAddress,
                MobileNumber = signupParameters.MobileNumber,
                IsActive = false,
                OtpCode = OtpHelper.GenerateOtp(6),
                OtpDate = DateTime.UtcNow,
            };

            user.PasswordHash = PasswordHelper.HashPassword(signupParameters.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            Console.WriteLine($"[OTP] Sent to {user.MobileNumber}: {user.OtpCode}");
            return Ok(new { user.Id });
        }

        [HttpPost("SignupResendOtp")]
        public IActionResult SignupResendOtp([FromBody] SignupUserParameters signupUserParameters)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == signupUserParameters.Id);

            if (user == null)
                return BadRequest("UserNotFound");

            var otpTimeOut = _configuration.GetValue("OtpTimeOut", 2);// default 2 mins
            if ((DateTime.UtcNow - user.OtpDate)?.TotalMinutes < otpTimeOut)
            return BadRequest("OtpAlreadySent");

            user.OtpCode = OtpHelper.GenerateOtp(6);
            user.OtpDate = DateTime.UtcNow;
            _context.SaveChanges();
            Console.WriteLine($"[OTP] Sent to {user.MobileNumber}: {user.OtpCode}");

            return Ok();
        }

        [HttpPost("SignupVerifyOtp")]
        public IActionResult SignupVerifyOtp([FromBody] SignupVerifyOtpParameters signupVerifyOtpParameters)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == signupVerifyOtpParameters.Id);
            if (user == null) return BadRequest("UserNotFound");

            var otpTimeOut = _configuration.GetValue("OtpTimeOut", 2); // 2 mins
            if ((DateTime.UtcNow - user.OtpDate)?.TotalMinutes > otpTimeOut)
                return BadRequest("OtpExpired");

            if (user.OtpCode != signupVerifyOtpParameters.OtpCode)
                return BadRequest("OtpNotMatched");

            user.IsActive = true;
            _context.SaveChanges();

            return Ok("AccountVerified");
        }
    }
}
