using System;
using Microsoft.AspNetCore.Authorization;

namespace Ombi.Auth.Policy
{
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public HasPermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }
}