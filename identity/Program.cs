using Identity.API.Middleware;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Imp;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.Application.UOW;
using Identity.DAL;
using Identity.Domain.Entities;
using Identity.Infrastructure.EmailServices;

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

        #region Enities
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IAsyncRepository<Permission>, AsyncReposatory<Permission>>();
        builder.Services.AddScoped<IAsyncRepository<RolePermission>, AsyncReposatory<RolePermission>>();
        builder.Services.AddScoped<IAsyncRepository<OTPCode>, AsyncReposatory<OTPCode>>();
        builder.Services.AddScoped<IAsyncRepository<OTPTry>, AsyncReposatory<OTPTry>>();
        builder.Services.AddScoped<IAsyncRepository<EmailVerification>, AsyncReposatory<EmailVerification>>();
        builder.Services.AddScoped<IAsyncRepository<EmailBody>, AsyncReposatory<EmailBody>>();
        builder.Services.AddScoped<IAsyncRepository<UserToken>, AsyncReposatory<UserToken>>();
        #endregion

        #region Services
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserServices, UsersServices>();
        builder.Services.AddScoped<ITokenService,TokenService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IPolicyStore, InMemoryPolicyStore>();
        builder.Services.AddScoped<IOTPService, OTPService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ILoginService, LoginService>();
        #endregion

        var jwtSection = builder.Configuration.GetSection("JwtSettings");
        builder.Services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()!;
        if (jwtSettings.SingleSignon == true)
        {
            IServiceCollection serviceCollection = builder.Services.AddSingleton<IConnectionMultiplexer>(
                 ConnectionMultiplexer.Connect("localhost:6379"));
;
        }
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
            app.UseMiddleware<SingleSigninMiddleware>();

        app.MapControllers();
        app.Run();
    }
}