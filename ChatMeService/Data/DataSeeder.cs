using System.Linq;
using System.Threading.Tasks;
using ChatMeService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ChatMeService.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IConfiguration Configuration { get; }

        public DataSeeder(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            Configuration = configuration;
        }

        public async Task SeedAsync()
        {
            if (!Program.AppStarted)
            {
                return;
            }

            if (Configuration["Connection"] == "SQLite")
            {
                db.Database.EnsureCreated();
            }

            // Add Roles
            if (!db.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
                await _roleManager.CreateAsync(new IdentityRole { Name = "User" });
            }

            // Add User
            if (!db.Users.Any())
            {
                var adminUser = new ApplicationUser()
                {
                    Email = Configuration["InitAdmin:Email"],
                    UserName = Configuration["InitAdmin:Email"],
                };

                var adminResult = await _userManager.CreateAsync(adminUser, Configuration["InitAdmin:Password"]);

                if (adminResult.Succeeded)
                {
                    adminUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(adminUser);

                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    await _userManager.AddToRoleAsync(adminUser, "User");
                }
            }
        }
    }
}