using ProjetFinEtude.Constants;
using ProjetFinEtude.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.Seeds
{
    public static class DefaultUsers
    {

        public async static Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            var defaultUser = new ApplicationUser
            {
                UserName = "idriss"
            };
            var adminUser = await userManager.FindByNameAsync(defaultUser.UserName);
            if (adminUser == null)
            {
                var test = await userManager.CreateAsync(defaultUser, "pass12345");
                await userManager.AddToRolesAsync(defaultUser,
                    new List<string> {
                        Roles.Admin.ToString(),
                        Roles.Student.ToString(),
                        Roles.Teacher.ToString(),
                        Roles.Parent.ToString() });
            }
        }
    }
}
