using Identity.Application.DTO;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Imp;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LogInController(ILoginService loginService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
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
