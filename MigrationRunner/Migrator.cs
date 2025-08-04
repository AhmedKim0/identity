using Identity.DAL;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;





internal class Migrator
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Optional: Load appsettings.json if you want
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("Default");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddIdentity<AppUser, AppRole>() // ✅ Registers UserManager, RoleManager
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
            })
            .Build();
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

            var creator = db.Database.GetService<IRelationalDatabaseCreator>();

            if (!await creator.ExistsAsync())
            {
                Console.WriteLine("Database does not exist. Running migrations...");
                await db.Database.MigrateAsync();
            }
            else
            {
                Console.WriteLine("Database already exists. Skipping migration.");
            }

            var permissions = new[]
                        {
                    "permission.getall","permission.getbyid","login.login","login.refresh-token","permission.create",
                    "permission.update","permission.delete","permission.assign","permission.getpermissionsbyrole",
                    "role.getall","role.getbyid","role.create", "role.delete","role.AssignRolesToUser","role.assigntouser","role.removefromuser",
                    "user.createuser","user.updateuser","user.deleteuser"
                };

            var existing = db.Permissions.Select(p => p.Name).ToList();
            var toAdd = permissions.Except(existing).Select(name => new Permission { Name = name }).ToList();

            if (toAdd.Any())
            {
                db.Permissions.AddRange(toAdd);
                db.SaveChanges();
                Console.WriteLine($"Added {toAdd.Count} new permissions.");
            }
            else
            {
                Console.WriteLine("No new permissions to add.");
            }
            var roles = new List<AppRole> () {new AppRole{Name="admin" }, new AppRole { Name = "norole" } };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                    Console.WriteLine($"Role '{role.Name}' created.");
                }
            }
            var newuser = new AppUser() { Email="admin@admin.com",UserName= "admin" };
            var user = await userManager.FindByEmailAsync("admin@admin.com");
            if (user==null)
            {
               var createduser= await userManager.CreateAsync(newuser,"Asd1236@");
               await  userManager.AddToRoleAsync(newuser, "admin");
                Console.WriteLine("user admin created and added to admin role.");

            }


        }

        Console.WriteLine("Done.");
    }
}