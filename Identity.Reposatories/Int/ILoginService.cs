using Identity.Application.DTO;
using Identity.Application.DTO.LoginDTOs;

namespace Identity.Application.Int
{
    public interface ILoginService
    {
        Task<Response<bool>> IsLoggedinAsync(LoginDTO model);
        Task<Response<TokenDTO>> LoginAsync(LoginDTO model);
        Task<Response<TokenDTO>> LogoutAsync(string accessToken);
        Task<Response<TokenDTO>> RefreshTokenAsync(RefreshTokenDTO model);
    }
}