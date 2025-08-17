using Identity.Application.DTO;
using Identity.Application.DTO.UserDTOs;
using Identity.Application.Int;

using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices)
        {
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(CreateUserDTO createUserDTO)
        {
            try
            {
                var result = await _userServices.CreateTest(createUserDTO.email, createUserDTO.password, createUserDTO.fullName);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, Response<UserDTO>.Failure(new Error(ex.Message)));
            }
        }
        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO updateUserDTO)
        {
            try
            {
                var result = await _userServices.UpdateUserAsync(updateUserDTO.Id , updateUserDTO.NewEmail, updateUserDTO.NewFullName);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, Response<UserDTO>.Failure(new Error(ex.Message)));
            }
        }
        [HttpDelete("DeleteUser")]

        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userServices.DeleteUserAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, Response<UserDTO>.Failure(new Error(ex.Message)));
            }
        }
    }
}
