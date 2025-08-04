using Identity.Application.DTO;
using Identity.Application.DTO.OTP;
using Identity.Application.Int;

using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("Generate")]
        public async Task<IActionResult> Generate([FromBody] string email)
        {
            try
            {
                var result = await _otpService.GenerateOtp(email);
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
                var result = await _otpService.ChangePassword(dto.Email, dto.Password, dto.Otp);
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
