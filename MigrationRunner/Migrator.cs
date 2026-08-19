using Identity.DAL;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Migrator
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile(
                    "appsettings.json",
                    optional: true,
                    reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString =
                    context.Configuration.GetConnectionString("Default");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddIdentity<AppUser, AppRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
            })
            .Build();

        using(var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var db = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

            var creator = db.Database.GetService<IRelationalDatabaseCreator>();

            if(!await creator.ExistsAsync())
            {
                Console.WriteLine("Database does not exist. Running migrations...");
                await db.Database.MigrateAsync();
            }
            else
            {
                Console.WriteLine("Database already exists. Skipping migration.");
            }

            var newpermissions = new Permission[]
            {
                // Login
                new Permission("login.isloggedin", "هل المستخدم مسجل دخول", "Is Logged In"),
                new Permission("login.login", "تسجيل الدخول", "Login"),
                new Permission("login.googlelogin", "تسجيل الدخول بجوجل", "Google Login"),
                new Permission("login.refresh-token", "تحديث التوكن", "Refresh Token"),
                new Permission("login.logout", "تسجيل الخروج", "Logout"),

                // OTP
                new Permission("otp.generate", "إرسال رمز التحقق", "Generate OTP"),
                new Permission("otp.verify", "تأكيد رمز التحقق", "Verify OTP"),
                new Permission("otp.changepassword", "تغيير كلمة المرور", "Change Password"),
                new Permission("otp.confirmemail", "تأكيد البريد الإلكتروني", "Confirm Email"),
                new Permission("otp.SendEmailConfim", "إرسال تأكيد البريد", "Send Email Confirmation"),
                new Permission("otp.confirmPhone", "تأكيد رقم الهاتف", "Confirm Phone"),

                // Permission
                new Permission("permission.getall", "عرض جميع الصلاحيات", "Get All Permissions"),
                new Permission("permission.getbyid", "عرض صلاحية معينة", "Get Permission By Id"),
                new Permission("permission.create", "إنشاء صلاحية", "Create Permission"),
                new Permission("permission.update", "تحديث صلاحية", "Update Permission"),
                new Permission("permission.delete", "حذف صلاحية", "Delete Permission"),
                new Permission("permission.assign", "إسناد صلاحية", "Assign Permission"),
                new Permission("permission.getpermissionsbyrole", "عرض صلاحيات الدور", "Get Permissions By Role"),

                // Role
                new Permission("role.getall", "عرض جميع الأدوار", "Get All Roles"),
                new Permission("role.getbyid", "عرض دور معين", "Get Role By Id"),
                new Permission("role.assignrolestouser", "إسناد الأدوار للمستخدم", "Assign Roles To User"),
                new Permission("role.create", "إنشاء دور", "Create Role"),
                new Permission("role.delete", "حذف دور", "Delete Role"),
                new Permission("role.assigntouser", "إسناد دور لمستخدم", "Assign Role To User"),
                new Permission("role.removefromuser", "إزالة دور من مستخدم", "Remove Role From User"),

                // User
                new Permission("user.createuser", "إنشاء مستخدم", "Create User"),
                new Permission("user.updateuser", "تحديث مستخدم", "Update User"),
                new Permission("user.deleteuser", "حذف مستخدم", "Delete User"),
                new Permission("user.getallusers", "عرض كل المستخدمين", "Get All Users"),
                new Permission("user.getuserbyid", "عرض مستخدم معين", "Get User By Id")
            };

            var existingPermissions = db.Permissions.ToList();

            // Update existing ones
            foreach(var perm in existingPermissions)
            {
                var newPerm = newpermissions
                    .FirstOrDefault(p => p.NameLogical == perm.NameLogical);

                if(newPerm != null)
                {
                    bool changed = false;

                    if(perm.NameAr != newPerm.NameAr)
                    {
                        perm.Update(
                            perm.NameLogical,
                            newPerm.NameAr,
                            newPerm.NameEn);

                        changed = true;
                    }
                    else if(perm.NameEn != newPerm.NameEn)
                    {
                        perm.Update(
                            perm.NameLogical,
                            perm.NameAr,
                            newPerm.NameEn);

                        changed = true;
                    }

                    if(changed)
                    {
                        db.Permissions.Update(perm);
                    }
                }
            }

            // Add missing ones
            var toAdd = newpermissions
                .Where(p =>
                    !existingPermissions.Any(
                        e => e.NameLogical == p.NameLogical))
                .ToList();

            if(toAdd.Any())
            {
                db.Permissions.AddRange(toAdd);

                Console.WriteLine(
                    $"✅ Added {toAdd.Count} new permissions.");
            }

            db.SaveChanges();

            Console.WriteLine("✅ Permissions sync completed.");

            var roles = new List<AppRole>
            {
                new AppRole(
                    "admin",
                    "مدير النظام",
                    "Administrator"),

                new AppRole(
                    "norole",
                    "بدون صلاحيات",
                    "No Role")
            };

            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);

                    Console.WriteLine(
                        $"Role '{role.Name}' created.");
                }
            }

            var newuser = new AppUser(
                "admin@admin.com",
                "admin");

            var user = await userManager
                .FindByEmailAsync("admin@admin.com");

            if(user == null)
            {
                var createduser = await userManager
                    .CreateAsync(newuser, "P@ssw0rd");

                await userManager.AddToRoleAsync(
                    newuser,
                    "admin");

                await userManager.ConfirmEmailAsync(
                    newuser,
                    await userManager.GenerateEmailConfirmationTokenAsync(newuser));

                Console.WriteLine(
                    "user admin created and added to admin role.");
            }
        }

        Console.WriteLine("Done.");
    }
}