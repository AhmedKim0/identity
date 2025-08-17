using Identity.Application.DTO;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Int;

using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IGoogleAuthService _googleAuthService;
        public LogInController(ILoginService loginService ,IGoogleAuthService googleAuthService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _googleAuthService=googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
        }
        [HttpPost("IsLoggedin")]

        public async Task<IActionResult> IsLoggedin([FromBody] LoginDTO model)
        {
            try
            {
                var result = await _loginService.IsLoggedinAsync(model);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<string>.Failure(new Error(ex.Message)));
            }

        }
        [HttpPost("Login")]

        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                var result = await _loginService.LoginAsync(model);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<string>.Failure(new Error(ex.Message)));
            }

        }
        [HttpGet("GoogleLogin/{authCode}")]
        public async Task<IActionResult> GoogleLogin( string authCode)
        {
            try
            {
                var result = await _googleAuthService.GetUserInfoAsync(authCode);
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<string>.Failure(new Error(ex.Message)));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO model)
        {
            try
            {
                var result = await _loginService.RefreshTokenAsync(model);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<string>.Failure(new Error(ex.Message)));
            }
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] string accessToken)
        {
            try
            {
                var result = await _loginService.LogoutAsync(accessToken);
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<string>.Failure(new Error(ex.Message)));
            }



        }
    }
}
