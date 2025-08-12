
using Azure.Core;

using Identity.Application.DTO;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Imp
{

    public class LoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IAsyncRepository<UserToken> _userTokenRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LoginService(UserManager<AppUser> userManager, ITokenService tokenService, JwtSettings jwtSettings, IAsyncRepository<UserToken> userTokenRepo, IUnitOfWork unitOfWork)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _userTokenRepo = userTokenRepo ?? throw new ArgumentNullException(nameof(userTokenRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Response<bool>> IsLoggedinAsync (LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Response<bool>.Failure(new Error("Invalid username or password"));
            var userToken = await _userTokenRepo.Dbset()
                        .FirstOrDefaultAsync(ut => ut.UserId == user.Id);
            if (userToken == null) 
              return  Response<bool>.SuccessResponse(false);
            return Response<bool>.SuccessResponse(true);


        }

        public async Task<Response<TokenDTO>> LoginAsync(LoginDTO model)
        {
            await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    return Response<TokenDTO>.Failure(new Error("Invalid username or password"));

                var (accessToken, refreshToken) = await _tokenService.GenerateTokens(user);

                if (_jwtSettings.SingleSignon)
                {
                    var userToken = await _userTokenRepo.Dbset()
                        .Where(ut => ut.UserId == user.Id).ToListAsync();

                    if (userToken != null)
                        _userTokenRepo.Dbset().RemoveRange(userToken);

                    var newUserToken = new UserToken
                    {
                        UserId = user.Id,
                        AccessToken = accessToken,
                        ATExpiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                        RefreshToken = refreshToken,
                        RTExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                    };

                    await _userTokenRepo.Dbset().AddAsync(newUserToken);
                }

                await _unitOfWork.CommitTransactionAsync();

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

                var user = await _userManager.FindByNameAsync(username!);
                if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                    return Response<TokenDTO>.Failure(new Error("Invalid refresh token"));

                var (newAccessToken, newRefreshToken) = await _tokenService.GenerateTokens(user);
                if (_jwtSettings.SingleSignon)
                {
                    var userToken = await _userTokenRepo.Dbset()
                        .Where(ut => ut.UserId == user.Id).ToListAsync();

                    if (userToken != null)
                        _userTokenRepo.Dbset().RemoveRange(userToken);

                    var newUserToken = new UserToken
                    {
                        UserId = user.Id,
                        AccessToken = newAccessToken,
                        ATExpiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                        RefreshToken = newRefreshToken,
                        RTExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                    };

                    await _userTokenRepo.Dbset().AddAsync(newUserToken);
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

        // use it in case SSO
        public async Task<Response<TokenDTO>> LogoutAsync(string accessToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return Response<TokenDTO>.Failure(new Error("Invalid token"));
            var username = principal.Identity?.Name;
            var user = await _userManager.FindByNameAsync(username!);
            if (user == null)
                return Response<TokenDTO>.Failure(new Error("User not found"));
            var userToken = await _userTokenRepo.Dbset().FirstOrDefaultAsync(ut => ut.UserId == user.Id);
            if (userToken != null)
            {
                _userTokenRepo.Dbset().Remove(userToken);
                await _userTokenRepo.SaveChangesAsync();
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