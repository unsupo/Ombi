using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Ombi.Auth.Core
{
    public class OmbiAuthorizationService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OmbiAuthorizationService(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(string permissionName)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var result = await _authorizationService.AuthorizeAsync(user, permissionName).ConfigureAwait(false);
            return result.Succeeded;
        }
    }
}