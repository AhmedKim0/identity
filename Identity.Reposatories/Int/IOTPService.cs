using Identity.Application.DTO;
using Identity.Application.DTO.OTP;
using Identity.Domain.Enums;

namespace Identity.Application.Int
{
    public interface IOTPService
    {
        Task<Response<bool>> ChangePassword(string Email, string Password, string Otp, OtpPurpose otp);
        Task<Response<string>> GenerateOtp(string email, OtpPurpose otp);
        Task<Response<bool>> VerifyOtpAsync(VerifyOtpDto dto);
        Task<Response<bool>> GenerateEmailVerificationTokenAsync(string email);
        Task<Response<string>> EmailConfirmAsync(string userId, string token);
    }
}