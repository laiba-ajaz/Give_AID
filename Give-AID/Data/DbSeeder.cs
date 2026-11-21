using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID.Data
{
    // Automatically seeds essential default data (such as the default Admin user)
    // so any team member or examiner running the application immediately has a working Admin account.
    public static class DbSeeder
    {
        // Asynchronously checks and seeds default administrative user account
        public static async Task SeedAsync(AppDBContext context)
        {
            // Verify database connection is available before attempting queries
            if (await context.Database.CanConnectAsync())
            {
                // Check if any user with Admin role exists
                var adminExists = await context.Users.AnyAsync(u => u.Role == "Admin");
                if (!adminExists)
                {
                    var hasher = new PasswordHasher<User>();
                    var admin = new User
                    {
                        FullName = "System Admin",
                        Email = "admin@give-aid.org",
                        Role = "Admin",
                        City = "Headquarters",
                        Occupation = "Administrator",
                        Organisation = "Give-AID Global",
                        PhoneNumber = "+1 (800) 448-3243",
                        Password = "",
                        ConfirmPassword = "Admin123!",
                        CreatedAt = DateTime.Now
                    };

                    // Securely hash the password using ASP.NET Core PasswordHasher
                    admin.Password = hasher.HashPassword(admin, "Admin123!");

                    await context.Users.AddAsync(admin);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
