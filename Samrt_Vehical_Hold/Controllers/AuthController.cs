using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samrt_Vehical_Hold.DTO.Login;
using Samrt_Vehical_Hold.DTO.ResetPassword;
using Samrt_Vehical_Hold.DTO.SignUp;
using Samrt_Vehical_Hold.DTO.UserInfo;
using Samrt_Vehical_Hold.Helpers.Service;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Interface;


namespace Samrt_Vehical_Hold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

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
            try
            {
        
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }


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
             .FirstOrDefault(x => x.UserName == loginParameters.UserNameOrEmail || x.EmailAddress == loginParameters.UserNameOrEmail);


            if (user == null)
                return BadRequest("UserNotFound");

            if (!user.IsActive)
                return BadRequest("AccountNotActive");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginParameters.Password);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest("InvalidPassword");


            var token = _jwtHelper.GenerateToken(user);
            return Ok(token);
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
        public async Task<IActionResult> ResetPassword([FromBody] DTO.ResetPassword.ResetPasswordRequest request)
        {
            var user = await _dataService.GetQuery<ApplicationUser>()
        .FirstOrDefaultAsync(u => u.EmailAddress == request.Email);

            if (user == null)
                return BadRequest("UserNotFound");

            var resetRequest = await _dataService.GetQuery<PasswordResetRequest>()
        .FirstOrDefaultAsync(r => r.UserId == user.Id && r.ResetCode == request.ResetCode && !r.IsUsed);

            if (resetRequest == null)
                return BadRequest("InvalidResetCode");

            if (resetRequest.ExpiryDate < DateTime.UtcNow)
                return BadRequest("ResetCodeExpired");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            resetRequest.IsUsed = true;

            await _dataService.SaveAsync();

            return Ok("PasswordResetSuccessful");
        }
       
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _dataService.GetByIdAsync<ApplicationUser>(userId.Value);
            if (user == null)
                return NotFound("UserNotFound");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest("InvalidOldPassword");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _dataService.UpdateAsync(user);
            await _dataService.SaveAsync();
            return Ok("PasswordChangedSuccessfully");
        }
        
        [Authorize]
        [HttpGet("GetProfile")]
        public async Task <IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _dataService.GetByIdAsync<ApplicationUser>(userId);

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
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _dataService.GetByIdAsync<ApplicationUser>(userId);

            if (user == null)
                return NotFound("UserNotFound");

            // Check if email is already used by another user
            var isEmailTaken = await _dataService.GetQuery<ApplicationUser>()
                .AnyAsync(u => u.EmailAddress == request.EmailAddress && u.Id != user.Id);
            if (isEmailTaken)
                return BadRequest("EmailAlreadyInUse");

            // Check if number is already used by another user
            var isNumberTaken = await _dataService.GetQuery<ApplicationUser>()
                .AnyAsync(u => u.MobileNumber == request.MobileNumber && u.Id != user.Id);
            if (isNumberTaken)
                return BadRequest("NumberAlreadyInUse");
            // Update data
            user.EmailAddress = request.EmailAddress;
            user.MobileNumber = request.MobileNumber;

            await _dataService.UpdateAsync(user);
            await _dataService.SaveAsync();

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
