using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Enumerations;
using MyStore.Models;
using System.Data;

namespace MyStore.DataSeeding
{
    public class DataSeeding
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<CompanyDBContext>();

            if (context != null)
            {
                try
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                    await InitialRoles(scope.ServiceProvider, context);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private static async Task InitialRoles(IServiceProvider serviceProvider, CompanyDBContext context)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            foreach (string role in Enum.GetNames(typeof(RolesEnum)))
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = role,
                        NormalizedName = role.ToUpper(),
                    });
                }
            }
            await InitialUsers(serviceProvider, context);
        }

        private static async Task InitialUsers(IServiceProvider serviceProvider, CompanyDBContext context)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var adminEmail = "lethinhachan18@gmail.com";
            var admin = new User
            {
                FullName = "Lê Thị Nhã Chân",
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                UserName = adminEmail,
                NormalizedUserName = adminEmail.ToUpper(),
                PhoneNumber = "0901089182",
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            
            if (!context.Users.Any(u => u.UserName == admin.UserName))
            {
                var result = await userManager.CreateAsync(admin, "Chan@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, RolesEnum.Admin.ToString());
                }
            }
        }
    }
}
