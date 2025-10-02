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
                var result = await _userServices.CreateUserAsync(createUserDTO.email, createUserDTO.password, createUserDTO.fullName, createUserDTO.phone);
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
                var result = await _userServices.UpdateUserAsync(updateUserDTO.Id , updateUserDTO.NewEmail, updateUserDTO.NewFullName,updateUserDTO.Phone);
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
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            {
                try
                {
                    var result = await _userServices.GetAllUsers();
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
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(int id)
        {
            {
                try
                {
                    var result = await _userServices.GetUserByIdAsync(id);
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
}
