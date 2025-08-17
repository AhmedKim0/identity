using Identity.Application.DTO;
using Identity.Application.DTO.OTP;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System.Data;
using System.Text;

namespace Identity.Application.Imp
{
    
    public class OTPService : IOTPService
    {

        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public OTPService()
        {

        }

        public OTPService(
            IConfiguration configuration, IUnitOfWork unitOfWork, IEmailService emailService)
        {

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        private string RandomOtpGenerator()
        {
            string otp = "0123456789qwertyuiopasdfghjklzxcvbnm";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                Random random = new Random();
                sb = sb.Append(otp[random.Next(37)]);
            }
            return sb.ToString();
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public string NormalizeEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var local = parts[0];
            var domain = parts[1].ToLower();

            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                // Remove everything after +
                var plusIndex = local.IndexOf('+');
                if (plusIndex >= 0)
                    local = local.Substring(0, plusIndex);

                // Remove dots (Gmail ignores dots in username)
                local = local.Replace(".", "");
            }

            return $"{local}@{domain}";
        }


        public async Task<Response<string>> GenerateOtp(string email)
        {
            await _unitOfWork.BeginTransactionAsync(default);
            try
            {
                var now = DateTime.UtcNow;
                if (!IsValidEmail(email))
                {
                    return Response<string>.Failure(new Error("Invalid email format."));
                }
                email = NormalizeEmail(email);
                var verification = await _unitOfWork.EmailVerifications.Dbset()
                    .Include(e => e.OTPCodes)
                    .FirstOrDefaultAsync(e => e.Email == email);

                if (verification == null)
                {
                    verification = new EmailVerification
                    {
                        Email = email,
                        IsVerified = false,
                    };
                    _unitOfWork.EmailVerifications.Dbset().Add(verification);
                    await _unitOfWork.EmailVerifications.SaveChangesAsync();
                }
                if (verification.OTPCodes.Count(x => x.CreatedAtUTC.Date == now.Date) >= int.Parse(_configuration["OTP:OTpPerDay"]))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Response<string>.Failure(new Error("You have reached the maximum number of OTP requests for today."));
                }

                string code = RandomOtpGenerator();
                var otp = new OTPCode
                {
                    EmailVerificationId = verification.Id,
                    Code = code,
                    IsUsed = false,
                    CreatedAtUTC = now,
                    IsExpired = false,
                    ExpireAt = now.AddMinutes(double.TryParse(_configuration["OTP:ExpireInMin"], out double mins) ? mins : 15),
                };

                _unitOfWork.OTPCodes.Dbset().Add(otp);
                await _unitOfWork.OTPCodes.SaveChangesAsync();
                var messege = await _emailService.GetEmailStructure(EmailStructure.OTP_English, email);
                var placeholders = new Dictionary<string, string>
                {
                    { "otpValue", code },
                    { "verificationCodeExpireAfterMins", _configuration["OTP:ExpireInMin"] }
                };
                messege = _emailService.ReplacePlaceholders(messege, placeholders);
                await _emailService.SendEmailAsync(messege);
                await _unitOfWork.CommitTransactionAsync();
                return Response<string>.SuccessResponse("OTP sent successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<string>.Failure(new Error("An error occurred while generating OTP: " + ex.Message));
            }
        }

        public async Task<Response<bool>> ChangePassword(string Email, string Password, string Otp)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var otp = await _unitOfWork.OTPCodes.Dbset().Include(x => x.EmailVerification).Include(x => x.OTPTries)
                    .Where(x => x.EmailVerification.Email == Email && x.Code == Otp)
                    .OrderByDescending(x => x.CreatedAtUTC).FirstOrDefaultAsync();


                if (otp == null || otp.ExpireAt < DateTime.UtcNow || otp.IsExpired == true)
                {
                    return Response<bool>.Failure(new Error("OTP not found or expired"));
                }

                var emailVerification = otp.EmailVerification;
                emailVerification.IsVerified = true;
                otp.IsUsed = true;
                otp.IsExpired = true;
                _unitOfWork.OTPCodes.Dbset().Update(otp);
                _unitOfWork.EmailVerifications.Dbset().Update(emailVerification);
                await _unitOfWork.CommitTransactionAsync();
                return Response<bool>.SuccessResponse(true);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<bool>.Failure(new Error("An error occurred while changing password: " + ex.Message));
            }

        }
        public async Task<Response<bool>> VerifyOtpAsync(VerifyOtpDto dto)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var otp = await _unitOfWork.OTPCodes.Dbset()
                    .Include(x => x.EmailVerification)
                    .Include(x => x.OTPTries)
                    .Where(x => x.EmailVerification.Email == dto.Email 
                                 &&(!x.IsUsed || !x.IsExpired) )
                    .OrderByDescending(x => x.CreatedAtUTC)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    return Response<bool>.Failure(new Error("OTP not found "));
                }
                // Check expiry
                if (otp.ExpireAt < DateTime.UtcNow)
                {
                    otp.IsExpired = true;
                    _unitOfWork.OTPCodes.Dbset().Update(otp);
                    await _unitOfWork.CommitTransactionAsync();
                    return Response<bool>.Failure(new Error("OTP expired."));
                }

                // Check tries

                int maxTries = int.Parse(_configuration["OTP:MaxTries"]);
                if (otp.OTPTries.Count >= maxTries)
                {
                    otp.IsExpired = true;
                    _unitOfWork.OTPCodes.Dbset().Update(otp);
                    await _unitOfWork.CommitTransactionAsync();
                    return Response<bool>.Failure(new Error("Too many attempts. OTP expired."));
                }



                // If wrong OTP
                if (otp.Code != dto.Otp)
                {
                    otp.OTPTries.Add(new OTPTry
                    {
                        TryAt = DateTime.UtcNow,
                        IsSuccess = false
                    });

                    if (otp.OTPTries.Count >= maxTries)
                        otp.IsExpired = true;

                    _unitOfWork.OTPCodes.Dbset().Update(otp);
                    await _unitOfWork.CommitTransactionAsync();
                    return Response<bool>.Failure(new Error("Invalid OTP."));
                }

                // Success
                otp.OTPTries.Add(new OTPTry
                {
                    TryAt = DateTime.UtcNow,
                    IsSuccess = true
                });
                otp.IsVerified = true;
                otp.IsUsed = false;
                otp.IsExpired = true;
                otp.EmailVerification.IsVerified = true;

                _unitOfWork.OTPCodes.Dbset().Update(otp);
                _unitOfWork.EmailVerifications.Dbset().Update(otp.EmailVerification);

                await _unitOfWork.CommitTransactionAsync();
                return Response<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<bool>.Failure(new Error("An error occurred while verifying OTP: " + ex.Message));
            }
        }

        public async Task<Response<bool>> UseOTPAsync(VerifyOtpDto dto)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var otp = await _unitOfWork.OTPCodes.Dbset()
                    .Include(x => x.EmailVerification)
                    .Include(x => x.OTPTries)
                    .Where(x => x.EmailVerification.Email == dto.Email &&
                                x.Code == dto.Otp &&
                                !x.IsUsed && x.IsVerified)

                    .OrderByDescending(x => x.CreatedAtUTC)
                    .FirstOrDefaultAsync();

                if (otp == null || otp.ExpireAt < DateTime.UtcNow)
                {
                    if (otp != null)
                    {
                        otp.OTPTries.Add(new OTPTry
                        {
                            TryAt = DateTime.UtcNow,
                            IsSuccess = false
                        });

                        otp.IsExpired = true;
                        _unitOfWork.OTPCodes.Dbset().Update(otp);
                        await _unitOfWork.CommitTransactionAsync();
                    }

                    return Response<bool>.Failure(new Error("OTP is invalid or expired"));
                }
                otp.IsUsed = true;
                _unitOfWork.OTPCodes.Dbset().Update(otp);
                _unitOfWork.EmailVerifications.Dbset().Update(otp.EmailVerification);

                await _unitOfWork.CommitTransactionAsync();
                return Response<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<bool>.Failure(new Error("An error occurred while verifying OTP: " + ex.Message));
            }


        }

    }
}
