using Identity.Application.DTO;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.Domain.Entities;
using Identity.Domain.IReposatory;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Application.Imp
{

    public class LoginService : ILoginService
    {
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService? _redisCacheService;
        public LoginService( ITokenService tokenService, JwtSettings jwtSettings, IUnitOfWork unitOfWork, IRedisCacheService? redisCacheService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _redisCacheService = redisCacheService;
        }

        public async Task<Response<bool>> IsLoggedinAsync (LoginDTO model)
        {
            try {
                model.Username=SharedFunctions.NormalizeEmail(model.Username);
            var user = await _unitOfWork._UserManager.FindByEmailAsync(model.Username);
            if (user == null || !await _unitOfWork._UserManager.CheckPasswordAsync(user, model.Password))
                return Response<bool>.Failure(new Error("Invalid username or password"));
            var userToken = await _redisCacheService?.GetAsync<UserToken>($"UserToken:{user.Id}");
            if (userToken == null) 
              return  Response<bool>.SuccessResponse(false);
            return Response<bool>.SuccessResponse(true);
            }
            catch
            {
                return Response<bool>.Failure(new Error("An error occurred while processing your request."));
            }


        }

        public async Task<Response<TokenDTO>> LoginAsync(LoginDTO model)
        {
            //await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                model.Username = SharedFunctions.NormalizeEmail(model.Username);

                var user = await _unitOfWork._UserManager.FindByEmailAsync(model.Username);
                if (user == null || !await _unitOfWork._UserManager.CheckPasswordAsync(user, model.Password))
                    return Response<TokenDTO>.Failure(new Error("Invalid username or password"));

                var (accessToken, refreshToken) = await _tokenService.GenerateTokens(user);

                if (_jwtSettings.SingleSession)
                {
                    _redisCacheService.GetAsync<UserToken>($"UserToken:{user.Id}").ContinueWith(async t =>
                    {
                        if (t.Result != null)
                        {
                           await  _redisCacheService.RemoveAsync($"UserToken:{user.Id}");
                        }
                    });
                    var newUserToken = new UserToken
                    {
                        UserId = user.Id,
                        AccessToken = accessToken,
                        ATExpiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                        RefreshToken = refreshToken,
                        RTExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                    };
                    await _redisCacheService.SetAsync($"UserToken:{user.Id}", newUserToken, TimeSpan.FromDays(_jwtSettings.AccessTokenExpirationMinutes));

                }


                return Response<TokenDTO>.SuccessResponse(new TokenDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
                });
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<TokenDTO>.Failure(new Error("An error occurred while processing your request."));
            }
        }

        public async Task<Response<TokenDTO>> RefreshTokenAsync(RefreshTokenDTO model)
        {
            try
            {

                var principal = GetPrincipalFromExpiredToken(model.AccessToken);
                var username = principal?.Identity?.Name;

                var user = await _unitOfWork._UserManager.FindByNameAsync(username!);
                if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                    return Response<TokenDTO>.Failure(new Error("Invalid refresh token"));

                var (newAccessToken, newRefreshToken) = await _tokenService.GenerateTokens(user);
                if (_jwtSettings.SingleSession)
                {
                    _redisCacheService.GetAsync<UserToken>($"UserToken:{user.Id}").ContinueWith(async t =>
                    {
                        if (t.Result != null)
                        {
                            await _redisCacheService.RemoveAsync($"UserToken:{user.Id}");
                        }
                    });
                    var newUserToken = new UserToken
                    {
                        UserId = user.Id,
                        AccessToken = newAccessToken,
                        ATExpiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                        RefreshToken = newRefreshToken,
                        RTExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                    };
                    await _redisCacheService.SetAsync($"UserToken:{user.Id}", newUserToken, TimeSpan.FromDays(_jwtSettings.AccessTokenExpirationMinutes));
                
                }

                return Response<TokenDTO>.SuccessResponse(new TokenDTO
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
                });
            }
            catch (Exception ex)
            {
                return Response<TokenDTO>.Failure(new Error(ex.Message));
            }
        }

        // use it in case SingleSession
        public async Task<Response<TokenDTO>> LogoutAsync(string accessToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return Response<TokenDTO>.Failure(new Error("Invalid token"));
            var username = principal.Identity?.Name;
            var user = await _unitOfWork._UserManager.FindByNameAsync(username!);
            if (user == null)
                return Response<TokenDTO>.Failure(new Error("User not found"));

            var userToken = await _redisCacheService.GetAsync<UserToken>($"UserToken:{user.Id}");
            if (userToken != null)
            {
                await _redisCacheService.RemoveAsync($"UserToken:{user.Id}");

            }
            return Response<TokenDTO>.SuccessResponse(new TokenDTO
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                ExpireAt = DateTime.UtcNow
            });
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // Accept expired token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            }
            catch
            {
                return null;
            }
        }

    }
}