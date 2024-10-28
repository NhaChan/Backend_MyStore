using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
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
            
            if(context != null)
            {
                try
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                    await InitialRoles(scope.ServiceProvider, context);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        private static async Task InitialRoles(IServiceProvider serviceProvider, CompanyDBContext context)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin" };

            foreach (string role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            await InitialUsers(serviceProvider, context, roles);
        }

        private static async Task InitialUsers(IServiceProvider serviceProvider, CompanyDBContext context, string[] roles)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var user = new User
            {
                FullName = "Nhã Chân",
                Email = "lethinhachan18@gmail.com",
                NormalizedEmail = "lethinhachan18@gmail.com",
                UserName = "lethinhachan18@gmail.com",
                NormalizedUserName = "lethinhachan18@gmail.com",
                PhoneNumber = "0901089182",
                //PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            if(!context.Users.Any(u => u.UserName == user.UserName))
            {
                var result = await userManager.CreateAsync(user, "Chan@123");
                if(result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }
    }
}
