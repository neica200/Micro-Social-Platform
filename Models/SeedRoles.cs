using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Micro_social_app.Models
{
    public static class SeedRoles
    {
        public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
  
                if (!await roleManager.Roles.AnyAsync(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}

