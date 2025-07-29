using Identity.Application.DTO;
using Identity.Application.DTO.PermissionDTOs;
using Identity.Application.Int;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService permissionService)
        {
            _service = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()

        {
            try
            {
                var result = await _service.GetAllAsync();

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<PermissionDTO>.Failure(new Error(ex.Message)));
            }
        }
        [Authorize]

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int id)

        {
            try
            {
                var result = await _service.GetByIdAsync(id);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<PermissionDTO>.Failure(new Error(ex.Message)));
            }
        }

        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] string name)

        {
            try
            {
                var result = await _service.CreateAsync(name);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<PermissionDTO>.Failure(new Error(ex.Message)));
            }
        }

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> Update( PermissionDTO dto)

        {
            try
            {
                var result = await _service.UpdateAsync( dto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<PermissionDTO>.Failure(new Error(ex.Message)));
            }
        }

        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)

        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<bool>.Failure(new Error(ex.Message)));
            }
        }
        [Authorize]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsDTO dto)

        {
            try
            {
                var result = await _service.AssignPermissionsToRoleAsync(dto.RoleId, dto.PermissionIds);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<bool>.Failure(new Error(ex.Message)));
            }
        }

        [Authorize]
        [HttpGet("GetPermissionsByRole")]

        public async Task<IActionResult> GetPermissionsByRole(int roleId)

        {
            try
            {
                var result = await _service.GetPermissionsByRoleAsync(roleId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<PermissionDTO>.Failure(new Error(ex.Message)));
            }
        }

    }

}

