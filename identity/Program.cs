using Identity.API.Middleware;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Imp;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.DAL;
using Identity.Domain.Entities;
using Identity.Domain.IReposatory;
using Identity.Infrastructure.EmailServices;
using Identity.Infrastructure.Redis;
using Identity.Infrastructure.Reposatory;
using Identity.Infrastructure.Reposatory.Identity.Infrastructure.Reposatory;
using Identity.Infrastructure.UOW;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using StackExchange.Redis;

using System.Security.Claims;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register AppDbContext first
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

        // Register Identity only once with correct role
        builder.Services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        #region GoogleServices
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Cookies";
            options.DefaultSignInScheme = "Cookies";
            options.DefaultChallengeScheme = "Google";
        })
            .AddCookie("Cookies")
            .AddGoogle("Google", options =>
            {
                options.ClientId = builder.Configuration["Google:ClientId"];
                options.ClientSecret = builder.Configuration["Google:ClientSecret"];
                options.CallbackPath = builder.Configuration["Google:CallbackPath"]; // Google will redirect here after login
            });
        #endregion

        #region Enities
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
        builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        builder.Services.AddScoped<IOTPCodeRepository, OTPCodeRepository>();
        builder.Services.AddScoped<IOTPTryRepository, OTPTryRepository>();
        builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
        builder.Services.AddScoped<IEmailBodyRepository, EmailBodyRepository>();
        builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();

        #endregion

        #region Services
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserServices, UsersServices>();
        builder.Services.AddScoped<ITokenService,TokenService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IOTPService, OTPService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ILoginService, LoginService>();
        builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        builder.Services.AddHttpClient();

        #endregion

        var jwtSection = builder.Configuration.GetSection("JwtSettings");
        builder.Services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()!;
        #region RedisCache

        if (jwtSettings.SingleSession == true)
        {
            var redisConnectionString = builder.Configuration.GetSection("Redis");
            builder.Services.Configure<RedisSettings>(redisConnectionString);

            var redisSettings = redisConnectionString.Get<RedisSettings>()!;
            var redisHosts = redisSettings.Hosts.Select(h => $"{h.Host}:{h.Port}").ToArray();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {


                var configOptions = ConfigurationOptions.Parse(string.Join(",", redisHosts));
                configOptions.Password = redisSettings.Password;
                configOptions.Ssl = redisSettings.Ssl;
                configOptions.AbortOnConnectFail = false;
                configOptions.ConnectRetry = redisSettings.ConnectRetry;
                configOptions.ConnectTimeout = redisSettings.ConnectTimeout;
                configOptions.DefaultDatabase = redisSettings.Database;
                configOptions.AllowAdmin = redisSettings.AllowAdmin;
                configOptions.ResolveDns = redisSettings.ResolveDns;

                return ConnectionMultiplexer.Connect(configOptions);
            });

            builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

        }
        else
        {
            builder.Services.AddSingleton<IRedisCacheService>(_ => null!);
        }
        #endregion
        builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = builder.Configuration.GetSection("JwtSettings:Issuers").Get<string[]>(),

        ValidateAudience = false,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(key),

        RoleClaimType = ClaimTypes.Role
    };
});

        builder.Services.AddAuthorization();


        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Account API",
            });

            // ✅ Security Definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: Bearer abc123"
            });

            // ✅ Security Requirement
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
        });


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowOrigin", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer(); 
        builder.Services.AddSwaggerGen();

        var app = builder.Build();





        // Swagger for development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();

        app.UseAuthorization();
        if (builder.Configuration.GetValue<bool>("UseAuthMiddleware"))
        app.UseMiddleware<DynamicAuthorizationMiddleware>();
        if(builder.Configuration.GetValue<bool>("JwtSettings:SingleSignon"))
            app.UseMiddleware<SingleSessionMiddleware>();

        app.MapControllers();
        app.Run();
    }
}