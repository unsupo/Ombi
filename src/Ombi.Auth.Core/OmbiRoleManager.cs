using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Auth.Core.Models;

namespace Ombi.Auth.Core
{
    public class OmbiRoleManager : RoleManager<IdentityRole>
    {
        public OmbiRoleManager(Microsoft.AspNetCore.Identity.IRoleStore<IdentityRole> store,
            IEnumerable<IRoleValidator<IdentityRole>> roleValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            ILogger<Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>> logger)
            : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }

        public async Task<RolesViewModel> GetRoleAsync(string role)
        {
            var identityRole = await Roles.FirstOrDefaultAsync(x => x.Name.Equals(role, StringComparison.InvariantCultureIgnoreCase)).ConfigureAwait(false);
            if (identityRole == null)
            {
                return null;
            }
            var viewModel = new RolesViewModel
            {
                RoleName = identityRole.Name
            };

            return viewModel;
        }

        public async Task<IList<Claim>> GetPermissionsAsync(string role)
        {
            var identityRole = Roles.FirstOrDefault(x => x.Name.Equals(role));
            if (identityRole == null)
            {
                return null;
            }

            return await GetClaimsAsync(identityRole);
        }

        public async Task<IList<Claim>> GetPermissionsAsync(IEnumerable<string> roles)
        {
            var claims = new List<Claim>();
            foreach (var role in roles)
            {
                var p = await GetPermissionsAsync(role);
                if (p != null)
                {
                    claims.AddRange(p);
                }
            }

            return claims;
        }


        public async Task<IdentityResult> AddClaimsAsync(IdentityRole role, IList<Claim> claims, CancellationToken token)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            var roleStore = Store as IRoleClaimStore<IdentityRole>;
            foreach (var claim in claims)
            {
                await roleStore.AddClaimAsync(role, claim, token);
            }
            return await UpdateRoleAsync(role);
        }
    }
}
