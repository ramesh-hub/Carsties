using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityService.ProfileService
{
    /// <summary>
    /// Extends more profile info to be added to access-token
    /// </summary>
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            //users existing claims
            var existingClaims = await _userManager.GetClaimsAsync(user);

            //username claim
            var claims = new List<Claim>()
            {
                new Claim("username", user.UserName)
            };

            context.IssuedClaims.AddRange(claims);
            //Only add the Fullname claim from existing claims
            context.IssuedClaims.Add(existingClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name));

        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask; 
        }
    }
}
