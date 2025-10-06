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
using System.Linq;





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

            var newpermissions = new Permission[]
 {
    // Login
    new Permission { NameLogical = "login.isloggedin", NameAr = "هل المستخدم مسجل دخول", NameEn = "Is Logged In" },
    new Permission { NameLogical = "login.login", NameAr = "تسجيل الدخول", NameEn = "Login" },
    new Permission { NameLogical = "login.googlelogin", NameAr = "تسجيل الدخول بجوجل", NameEn = "Google Login" },
    new Permission { NameLogical = "login.refresh-token", NameAr = "تحديث التوكن", NameEn = "Refresh Token" },
    new Permission { NameLogical = "login.logout", NameAr = "تسجيل الخروج", NameEn = "Logout" },

    // OTP
    new Permission { NameLogical = "otp.generate", NameAr = "إرسال رمز التحقق", NameEn = "Generate OTP" },
    new Permission { NameLogical = "otp.verify", NameAr = "تأكيد رمز التحقق", NameEn = "Verify OTP" },
    new Permission { NameLogical = "otp.changepassword", NameAr = "تغيير كلمة المرور", NameEn = "Change Password" },
    new Permission { NameLogical = "otp.confirmemail", NameAr = "تأكيد البريد الإلكتروني", NameEn = "Confirm Email" },
    new Permission { NameLogical = "otp.SendEmailConfim", NameAr = "إرسال تأكيد البريد", NameEn = "Send Email Confirmation" },
    new Permission { NameLogical = "otp.confirmPhone", NameAr = "تأكيد رقم الهاتف", NameEn = "Confirm Phone" },

    // Permission
    new Permission { NameLogical = "permission.getall", NameAr = "عرض جميع الصلاحيات", NameEn = "Get All Permissions" },
    new Permission { NameLogical = "permission.getbyid", NameAr = "عرض صلاحية معينة", NameEn = "Get Permission By Id" },
    new Permission { NameLogical = "permission.create", NameAr = "إنشاء صلاحية", NameEn = "Create Permission" },
    new Permission { NameLogical = "permission.update", NameAr = "تحديث صلاحية", NameEn = "Update Permission" },
    new Permission { NameLogical = "permission.delete", NameAr = "حذف صلاحية", NameEn = "Delete Permission" },
    new Permission { NameLogical = "permission.assign", NameAr = "إسناد صلاحية", NameEn = "Assign Permission" },
    new Permission { NameLogical = "permission.getpermissionsbyrole", NameAr = "عرض صلاحيات الدور", NameEn = "Get Permissions By Role" },

    // Role
    new Permission { NameLogical = "role.getall", NameAr = "عرض جميع الأدوار", NameEn = "Get All Roles" },
    new Permission { NameLogical = "role.getbyid", NameAr = "عرض دور معين", NameEn = "Get Role By Id" },
    new Permission { NameLogical = "role.assignrolestouser", NameAr = "إسناد الأدوار للمستخدم", NameEn = "Assign Roles To User" },
    new Permission { NameLogical = "role.create", NameAr = "إنشاء دور", NameEn = "Create Role" },
    new Permission { NameLogical = "role.delete", NameAr = "حذف دور", NameEn = "Delete Role" },
    new Permission { NameLogical = "role.assigntouser", NameAr = "إسناد دور لمستخدم", NameEn = "Assign Role To User" },
    new Permission { NameLogical = "role.removefromuser", NameAr = "إزالة دور من مستخدم", NameEn = "Remove Role From User" },

    // User
    new Permission { NameLogical = "user.createuser", NameAr = "إنشاء مستخدم", NameEn = "Create User" },
    new Permission { NameLogical = "user.updateuser", NameAr = "تحديث مستخدم", NameEn = "Update User" },
    new Permission { NameLogical = "user.deleteuser", NameAr = "حذف مستخدم", NameEn = "Delete User" },
    new Permission { NameLogical = "user.getallusers", NameAr = "عرض كل المستخدمين", NameEn = "Get All Users" },
    new Permission { NameLogical = "user.getuserbyid", NameAr = "عرض مستخدم معين", NameEn = "Get User By Id" }
 };


            var existingPermissions = db.Permissions.ToList();

            // Update existing ones (if translation changed)
            foreach (var perm in existingPermissions)
            {
                var newPerm = newpermissions.FirstOrDefault(p => p.NameLogical == perm.NameLogical);
                if (newPerm != null)
                {
                    bool changed = false;

                    if (perm.NameAr != newPerm.NameAr)
                    {
                        perm.NameAr = newPerm.NameAr;
                        changed = true;
                    }

                    if (perm.NameEn != newPerm.NameEn)
                    {
                        perm.NameEn = newPerm.NameEn;
                        changed = true;
                    }

                    if (changed)
                        db.Permissions.Update(perm);
                }
            }

            // Add missing ones
            var toAdd = newpermissions
                .Where(p => !existingPermissions.Any(e => e.NameLogical == p.NameLogical))
                .ToList();

            if (toAdd.Any())
            {
                db.Permissions.AddRange(toAdd);
                Console.WriteLine($"✅ Added {toAdd.Count} new permissions.");
            }

            db.SaveChanges();
            Console.WriteLine("✅ Permissions sync completed.");
            var roles = new List<AppRole>()
                        {
                            new AppRole
                            {
                                Name = "admin",
                                NameAr = "مدير النظام",
                                NameEn = "Administrator"
                            },
                            new AppRole
                            {
                                Name = "norole",
                                NameAr = "بدون صلاحيات",
                                NameEn = "No Role"
                            }
                        };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                    Console.WriteLine($"Role '{role.Name}' created.");
                }
            }
            var newuser = new AppUser() { Email = "admin@admin.com", UserName = "admin" };
            var user = await userManager.FindByEmailAsync("admin@admin.com");
            if (user == null)
            {
                var createduser = await userManager.CreateAsync(newuser, "P@ssw0rd");
                await userManager.AddToRoleAsync(newuser, "admin");
                await userManager.ConfirmEmailAsync(newuser, await userManager.GenerateEmailConfirmationTokenAsync(newuser));
                Console.WriteLine("user admin created and added to admin role.");

            }
            var otpEnglishBody = @"
                                <html>
                                    <head>
                                        <meta charset='UTF-8'>
                                    </head>
                                    <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                                        <p>Your OTP code is: <b>{{{otpValue}}}</b>.</p>
                                        <p>It will expire in <b>{{{verificationCodeExpireAfterMins}}}</b> minutes.</p>
                                    </body>
                                </html>
                                ";

            var otpArabicBody = @"
                                <html>
                                    <head>
                                        <meta charset='UTF-8'>
                                    </head>
                                    <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333; direction: rtl; text-align: right;'>
                                        <p>رمز التحقق الخاص بك هو: <b>{{{otpValue}}}</b>.</p>
                                        <p>ستنتهي صلاحيته خلال <b>{{{verificationCodeExpireAfterMins}}}</b> دقائق.</p>
                                    </body>
                                </html>
                                ";

            // Create objects and add them via EF
            var emailBodies = new List<EmailBody>
{
    new EmailBody
    {
        Name = "OTP_English",
        Subject = "OTP",
        Body = otpEnglishBody,
        IsDeleted = false
    },
    new EmailBody
    {
        Name = "OTP_Arabic",
        Subject = "رمز التحقق",
        Body = otpArabicBody,
        IsDeleted = false
    }
};

            db.emailBodies.AddRange(emailBodies);
            await db.SaveChangesAsync();
            Console.WriteLine("Email bodies added successfully.");



        }

        Console.WriteLine("Done.");
    }
}