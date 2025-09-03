using Identity.Application.DTO;
using Identity.Application.DTO.OTP;
using Identity.Application.Int;
using Identity.Application;

using Microsoft.AspNetCore.Mvc;
using Identity.Application.Imp;
using Identity.Domain.Enums;
using static System.Net.WebRequestMethods;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTPController : ControllerBase
    {
        private readonly IOTPService _otpService;

        public OTPController(IOTPService otpService)
        {
            _otpService = otpService;

        }

        [HttpGet("Generate")]
        public async Task<IActionResult> Generate( string email,  OtpPurpose otp)
        {
            try
            {
                var result = await _otpService.GenerateOtp(email,  otp);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<string>.Failure(new Error(ex.Message)));
            }
        }

        [HttpPost("Verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyOtpDto dto)
        {
            try
            {
                var result = await _otpService.VerifyOtpAsync(dto);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<bool>.Failure(new Error(ex.Message)));
            }
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                var result = await _otpService.ChangePassword(dto.Email, dto.Password, dto.Otp,dto.otpPurpose);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<bool>.Failure(new Error(ex.Message)));
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                var result = await _otpService.EmailConfirmAsync(userId, token);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<bool>.Failure(new Error(ex.Message)));
            }
        }
        [HttpPost("SendEmailConfim")]
        public async Task<IActionResult> SendEmailConfim(string email)
        {
            try
            {
                var result = await _otpService.GenerateEmailVerificationTokenAsync(email);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<bool>.Failure(new Error(ex.Message)));
            }


        }
    }
}