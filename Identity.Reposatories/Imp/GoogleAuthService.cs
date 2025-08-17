
using Identity.Application.DTO;
using Identity.Application.DTO.GoogleDTOs;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.DTO.UserDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.DAL;
using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System.Data;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Identity.Application.Imp
{


    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;
        private readonly IUserServices _userServices;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        private readonly AppDbContext _db;

        public GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork
            , IUserServices userServices, IRoleService roleService, ITokenService tokenService,JwtSettings jwtSettings)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userServices = userServices?? throw new ArgumentNullException(nameof(userServices)) ;
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        public string GetGoogleAuthUrl()
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var redirectUri = _configuration["Authentication:Google:RedirectUri"];
            var scope = "openid profile email";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                      $"client_id={clientId}&" +
                      $"redirect_uri={redirectUri}&" +
                      $"response_type=code&" +
                      $"scope={scope}";

            return url;
        }

        public async Task<Response<TokenDTO>> GetUserInfoAsync(string code)
        {

            try
            {
                var clientId = _configuration["Google:ClientId"];
                var clientSecret = _configuration["Google:ClientSecret"];
                var redirectUri = _configuration["Google:RedirectUri"];

                var client = _httpClientFactory.CreateClient();

                // Step 1: Exchange code for access token
                var tokenResponse = await client.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                {"code", Uri.UnescapeDataString(code)},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"redirect_uri", redirectUri},
                {"grant_type", "authorization_code"}
                    })
                );
                var responseString = await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Google token exchange failed: {tokenResponse.StatusCode} - {responseString}");
                }

                //tokenResponse.EnsureSuccessStatusCode();
                var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(
                    await tokenResponse.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (tokenData.AccessToken == null) return null;

                // Step 2: Get user info
                client.DefaultRequestHeaders.Authorization =
             new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

                var userResponse = await client.GetAsync("https://openidconnect.googleapis.com/v1/userinfo");

                var responseContent = await userResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"User Info Raw Response: {responseContent}");

                userResponse.EnsureSuccessStatusCode();

                var userData = JsonSerializer.Deserialize<GoogleUserInfo>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                var userexist = await _unitOfWork._UserManager.FindByEmailAsync(userData.Email);
                if (userexist == null)
                { // implemnet registration
                    await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

                    var user=await CreateGoogleUser(userData.Email, "A@s12" + Guid.NewGuid().ToString(), userData.Name);
                    if (user == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Response<TokenDTO?>.Failure(new Error("User not created successfully"));
                    }
                    var tokens=await _tokenService.GenerateTokens(user);
                    await _unitOfWork.CommitTransactionAsync();
                    var response = new TokenDTO 
                    {
                        AccessToken = tokens.accessToken,
                        RefreshToken = tokens.refreshToken,
                        ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),

                    };
                    return Response<TokenDTO?>.SuccessResponse(response);
                }
                    // User already exists, generate tokens
                    var token = await _tokenService.GenerateTokens(userexist);
                    var existUserResponse = new TokenDTO
                    {
                        AccessToken = token.accessToken,
                        RefreshToken = token.refreshToken,
                        ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                    };
                    return Response<TokenDTO?>.SuccessResponse(existUserResponse);
                }
            catch (Exception ex)
            {
                // Log the exception
                await _unitOfWork.RollbackTransactionAsync();

                return Response<TokenDTO?>.Failure(new Error("An error occurred while fetching user info from Google."));
            }
        }
        private async Task<AppUser?> CreateGoogleUser(string email, string password, string fullName)
        {
            var userExists = await _unitOfWork._UserManager.FindByEmailAsync(email);
            if (userExists != null)
            {
                throw new Exception("User already exists");
            }
            if (!IsValidEmail(email))
            {
                throw new Exception("Invalid email format");
            }
            email = NormalizeEmail(email);

            var user = new AppUser
            {
                UserName = email,
                Email = email
            };

            var result = await _unitOfWork._UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error
                (
                    e.Description,
                     e.Code
                )).ToList();

                throw new Exception("User not created successfully: " + string.Join(", ", errors.Select(e => e.Message)));
            }
            List<int> rolesIds = _configuration.GetSection("JwtSettings:DefaultUserRoleIds").Get<int[]>().ToList();
            var roles = await _unitOfWork._RoleManager.Roles
                .Where(r => rolesIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();
            var addToRolesResult = await _unitOfWork._UserManager.AddToRolesAsync(user, roles.Select(r => r.ToString()));



            return user;
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public string NormalizeEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var local = parts[0];
            var domain = parts[1].ToLower();

            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                // Remove everything after +
                var plusIndex = local.IndexOf('+');
                if (plusIndex >= 0)
                    local = local.Substring(0, plusIndex);

                // Remove dots (Gmail ignores dots in username)
                local = local.Replace(".", "");
            }

            return $"{local}@{domain}";
        }

    }




}