using Identity.Application.DTO;
using Identity.Application.DTO.OTP;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.Application.UOW;
using Identity.Domain.Entities;
using Identity.Infrastructure.EmailServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Repos
{
    public class OTPService : IOTPService
    {
        private readonly IAsyncRepository<EmailVerification> _emailVerificationRepo;
        private readonly IAsyncRepository<OTPCode> _otpCodeRepo;
        private readonly IAsyncRepository<OTPTry> _otpTryRepo;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public OTPService(IAsyncRepository<EmailVerification> emailVerificationRepo, IAsyncRepository<OTPCode> otpCodeRepo, IAsyncRepository<OTPTry> otpTryRepo,
            IConfiguration configuration, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _emailVerificationRepo = emailVerificationRepo ?? throw new ArgumentNullException(nameof(emailVerificationRepo));
            _otpCodeRepo = otpCodeRepo ?? throw new ArgumentNullException(nameof(otpCodeRepo));
            _otpTryRepo = otpTryRepo ?? throw new ArgumentNullException(nameof(otpTryRepo));
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
        private  bool IsValidEmail(string email)
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

        public async Task<Response<string>> GenerateOtp(string email)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                if (!IsValidEmail(email))
                {
                    return Response<string>.Failure(new Error("Invalid email format."));
                }
                var verification = await _emailVerificationRepo.Dbset()
                    .Include(e => e.OTPCodes)
                    .FirstOrDefaultAsync(e => e.Email == email);

                if (verification == null)
                {
                    verification = new EmailVerification
                    {
                        Email = email,
                        IsVerified = false,
                    };
                    _emailVerificationRepo.Dbset().Add(verification);
                    await _emailVerificationRepo.SaveChangesAsync();
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
                    IsUsed=false,
                    CreatedAtUTC=now,
                    IsExpired = false,
                    ExpireAt = now.AddMinutes((double.TryParse(_configuration["OTP:ExpireInMin"], out double mins) ? mins : 15)),
                };

                  _otpCodeRepo.Dbset().Add(otp);
                await _otpCodeRepo.SaveChangesAsync();
                var messege=await _emailService.GetEmailStructure(EmailStructure.OTP_English, email);
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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var otp = await _otpCodeRepo.Dbset().Include(x => x.EmailVerification).Include(x => x.OTPTries)
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
                _otpCodeRepo.Dbset().Update(otp);
                _emailVerificationRepo.Dbset().Update(emailVerification);
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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var otp = await _otpCodeRepo.Dbset()
                    .Include(x => x.EmailVerification)
                    .Include(x => x.OTPTries)
                    .Where(x => x.EmailVerification.Email == dto.Email &&
                                x.Code == dto.Otp &&
                                !x.IsExpired)
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
                        _otpCodeRepo.Dbset().Update(otp);
                        await _unitOfWork.CommitTransactionAsync();
                    }

                    return Response<bool>.Failure(new Error("OTP is invalid or expired"));
                }

                // Check if reached max tries
                int maxTries = int.Parse(_configuration["OTP:MaxTries"]);
                if (otp.OTPTries.Count >= maxTries)
                {
                    otp.IsExpired = true;
                    _otpCodeRepo.Dbset().Update(otp);
                    await _unitOfWork.CommitTransactionAsync();
                    return Response<bool>.Failure(new Error("Too many attempts. OTP expired."));
                }

                otp.OTPTries.Add(new OTPTry
                {
                    TryAt = DateTime.UtcNow,
                    IsSuccess = true
                });

                otp.IsUsed = false;
                otp.IsExpired = false;
                otp.EmailVerification.IsVerified = true;

                _otpCodeRepo.Dbset().Update(otp);
                _emailVerificationRepo.Dbset().Update(otp.EmailVerification);

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
