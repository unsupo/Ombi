using Microsoft.AspNetCore.Authorization;

namespace Ombi.Auth.Policy
{
    public class AuthorizeEntityAttribute : AuthorizeAttribute
    {
        public AuthorizeEntityAttribute() : this(string.Empty)
        {

        }

        public AuthorizeEntityAttribute(PermissionAction action, string entity) : this($"{action}{entity}")
        {

        }

        public AuthorizeEntityAttribute(PermissionAction action) : this(action.ToString())
        {

        }

        public AuthorizeEntityAttribute(string policy) : base(policy)
        {
            AuthenticationSchemes = "Bearer";
        }
    }
}