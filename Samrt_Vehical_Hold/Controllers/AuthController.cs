using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Samrt_Vehical_Hold.Data;
using Samrt_Vehical_Hold.DTO.Login;
using Samrt_Vehical_Hold.DTO.ResetPassword;
using Samrt_Vehical_Hold.DTO.SignUp;
using Samrt_Vehical_Hold.DTO.UserInfo;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Interface;
using System.Net;
using System.Security.Claims;


namespace Samrt_Vehical_Hold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        private readonly IDataService _dataService;
        public AuthController(IDataService dataService,IConfiguration configuration,IPasswordHasher<ApplicationUser> passwordHasher,JwtHelper jwtHelper) : base(dataService)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
        }


        [HttpPost("SignUp")]
        public async Task <IActionResult>SignUp([FromBody] SignupParameters signupParameters) 
        {
            var errors = new List<string>();
            if (_dataService.GetQuery<ApplicationUser>().Any(x => x.UserName == signupParameters.UserName))
                errors.Add("UserNameUsed");
            if (_dataService.GetQuery<ApplicationUser>().Any(x => x.MobileNumber == signupParameters.MobileNumber))
                errors.Add("MobileNumberUsed");
            if (_dataService.GetQuery<ApplicationUser>().Any(x => x.EmailAddress == signupParameters.EmailAddress))
                errors.Add("EmailAddressUsed");
            if (_dataService.GetQuery<ApplicationUser>().Any(x => x.NationalNumber == signupParameters.NationalNumber))
                errors.Add("NationalNumberUsed");
        

            if (errors.Any())
                return BadRequest(errors);
            var user = new ApplicationUser
            {
                FullName = signupParameters.FullName,
                UserName = signupParameters.UserName,
                EmailAddress = signupParameters.EmailAddress,
                MobileNumber = signupParameters.MobileNumber,
                NationalNumber = signupParameters.NationalNumber,
                IsActive = false,
                OtpCode = OtpHelper.GenerateOtp(6),
                OtpDate = DateTime.UtcNow,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, signupParameters.Password);
            await _dataService.AddAsync(user);
            await _dataService.SaveAsync();
            Console.WriteLine($"[OTP] Sent to {user.MobileNumber}: {user.OtpCode}");
            return Ok(new { user.Id });
        }

        [HttpPost("SignupResendOtp")]
        public async Task <IActionResult>SignupResendOtp([FromBody] SignupUserParameters signupUserParameters)
        {
            var user = await GetUserByIdAsync(signupUserParameters.Id);

            if (user == null)
                return BadRequest("UserNotFound");

            var otpTimeOut = _configuration.GetValue("OtpTimeOut", 2);// default 2 mins
            if ((DateTime.UtcNow - user.OtpDate)?.TotalMinutes < otpTimeOut)
            return BadRequest("OtpAlreadySent");

            user.OtpCode = OtpHelper.GenerateOtp(6);
            user.OtpDate = DateTime.UtcNow;
            _dataService.SaveAsync();
            return Ok($"[OTP] Sent to {user.MobileNumber}: {user.OtpCode}");
        }

        [HttpPost("SignupVerifyOtp")]
        public async Task <IActionResult> SignupVerifyOtp([FromBody] SignupVerifyOtpParameters signupVerifyOtpParameters)
        {
            var user = await GetUserByIdAsync(signupVerifyOtpParameters.Id);

            if (user == null) return BadRequest("UserNotFound");

            var otpTimeOut = _configuration.GetValue("OtpTimeOut", 2); // 2 mins
            if ((DateTime.UtcNow - user.OtpDate)?.TotalMinutes > otpTimeOut)
                return BadRequest("OtpExpired");

            if (user.OtpCode != signupVerifyOtpParameters.OtpCode)
                return BadRequest("OtpNotMatched");

            user.IsActive = true;
            _dataService.SaveAsync();
            return Ok("AccountVerified");
        }

        [HttpGet("CheckNationalNumber/{userId}")]
        public async Task<IActionResult> CheckNationalNumber(Guid userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("UserNotFound");

            if (string.IsNullOrEmpty(user.NationalNumber))
                return Ok(new
                {
                    HasNationalNumber = false,
                    Message = "User has no National Number registered."
                });

            return Ok(new
            {
                HasNationalNumber = true,
                NationalNumber = user.NationalNumber,
                FullName = user.FullName,
                MobileNumber = user.MobileNumber,
                EmailAddress = user.EmailAddress
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginParameters loginParameters)
        {
            var user =  _dataService.GetQuery<ApplicationUser>()
             .FirstOrDefault(x => x.UserName == loginParameters.UserNameOrEmail);


            if (user == null)
                return BadRequest("UserNotFound");

            if (!user.IsActive)
                return BadRequest("AccountNotActive");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginParameters.Password);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest("InvalidPassword");


            var token = _jwtHelper.GenerateToken(user);
            return Ok(
                new 
                {
                    Token = token,
                    FullName = user.FullName

                });
        }

        // Email Sender Class (SMTP + MailKit) we Need that
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] DTO.ResetPassword.ForgotPasswordRequest request)
        {
            var user = _dataService.GetQuery<ApplicationUser>()
                .FirstOrDefault(u => u.EmailAddress == request.Email);
            if (user == null)
                return BadRequest("UserNotFound");

            var resetCode = new Random().Next(100000, 999999).ToString();

            var resetRequest = new PasswordResetRequest
            {
                UserId = user.Id,
                ResetCode = resetCode,
                ExpiryDate = DateTime.UtcNow.AddMinutes(15)
            };
            await _dataService.CreateAsync(resetRequest);

            return Ok("Otp Sent");
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] DTO.ResetPassword.ResetPasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailAddress == request.Email);
            if (user == null)
                return BadRequest("UserNotFound");

            var resetRequest = _context.PasswordResetRequests
                .FirstOrDefault(r => r.UserId == user.Id && r.ResetCode == request.ResetCode && !r.IsUsed);

            if (resetRequest == null)
                return BadRequest("InvalidResetCode");

            if (resetRequest.ExpiryDate < DateTime.UtcNow)
                return BadRequest("ResetCodeExpired");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            resetRequest.IsUsed = true;

            _context.SaveChanges();

            return Ok("PasswordResetSuccessful");
        }
        [Authorize]
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound("UserNotFound");

            var isOldPasswordValid = PasswordHelper.VerifyPassword(request.OldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
                return BadRequest("InvalidOldPassword");

            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            _context.SaveChanges();

            return Ok("PasswordChangedSuccessfully");
        }

        [Authorize]
        [HttpGet("GetProfile")]
        public async Task <IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = await _dataService.GetByIdAsync<ApplicationUser>(Guid.Parse(userId));

            if (user == null)
                return NotFound("UserNotFound");

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.UserName,
                user.EmailAddress,
                user.IsActive
            });
        }
        [Authorize]
        [HttpPost("UpdateProfile")]
        public async Task <IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = await _dataService.GetByIdAsync<ApplicationUser>(Guid.Parse(userId));

            if (user == null)
                return NotFound("UserNotFound");

            // Check if email is already used by another user
            var isEmailTaken = _context.Users.Any(u => u.EmailAddress == request.EmailAddress && u.Id.ToString() != userId);
            if (isEmailTaken)
                return BadRequest("EmailAlreadyInUse");

            // Update data
            user.EmailAddress = request.EmailAddress;
            user.MobileNumber = request.MobileNumber;

            _dataService.SaveAsync();

            return Ok("ProfileUpdatedSuccessfully");
        }
        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            return Ok("LoggedOutSuccessfully");
        }




    }
}
