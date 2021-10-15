using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ExampleProject.Web
{
    public class UserBootstrapService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserBootstrapService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task CreateDefaultUsers()
        {
            var infoUser = "info@example.com";
            var user = await _userManager.FindByEmailAsync(infoUser);
            if (user == null)
            {
                var result = await _userManager.CreateAsync(
                    new IdentityUser
                    {
                        UserName = infoUser,
                        Email = infoUser,
                        EmailConfirmed = true,
                    },
                    "Hase1234$");
            }
        }
    }
}