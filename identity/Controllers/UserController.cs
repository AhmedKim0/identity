using Identity.Reposatories.Repos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UsersServices _userServices;

        public UserController(UsersServices userServices)
        {
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        [Authorize(Policy = "Permission.User.Create")]
        [HttpGet]
        public async Task<IActionResult> CreateUser(string email, string password, string fullName)
        {
            try
            {
                var result = await _userServices.CreateUserAsync(email, password, fullName);
                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
