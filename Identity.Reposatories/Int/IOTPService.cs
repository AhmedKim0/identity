using Identity.Application.DTO;
using Identity.Application.DTO.OTP;

namespace Identity.Application.Int
{
    public interface IOTPService
    {
        Task<Response<bool>> ChangePassword(string Email, string Password, string Otp);
        Task<Response<string>> GenerateOtp(string email);
        Task<Response<bool>> VerifyOtpAsync(VerifyOtpDto dto);
        Task<Response<bool>> UseOTPAsync(VerifyOtpDto dto);

    }
}