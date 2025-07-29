using Identity.Application.DTO;
using Identity.Application.DTO.RoleDTOs;
using Identity.Application.Int;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<RoleDTO>.Failure(new Error(ex.Message)));
            }
        }
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var role = await _roleService.GetByIdAsync(id);
                if (role == null) return NotFound();
                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<RoleDTO>.Failure(new Error(ex.Message)));
            }
        }
        [Authorize]

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] string roleName)
        {
            try
            {
                var result = await _roleService.CreateAsync(roleName);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<RoleDTO>.Failure(new Error(ex.Message)));
            }
        }
        [Authorize]

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _roleService.DeleteAsync(id);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<RoleDTO>.Failure(new Error(ex.Message)));
            }
        }
        [Authorize]

        [HttpPost("AssignToUser")]
        public async Task<IActionResult> AssignToUser([FromBody] AssignRoleDTO dto)
        {
            try
            {
                var result = await _roleService.AssignRoleToUserAsync(dto.UserId, dto.RoleName);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<RoleDTO>.Failure(new Error(ex.Message)));
            }
        }
        [Authorize]

        [HttpPost("RemoveFromUser")]
        public async Task<IActionResult> RemoveFromUser([FromBody] AssignRoleDTO dto)
        {
            try
            {
                var result = await _roleService.RemoveRoleFromUserAsync(dto.UserId, dto.RoleName);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<RoleDTO>.Failure(new Error(ex.Message)));
            }
        }
    }
}
