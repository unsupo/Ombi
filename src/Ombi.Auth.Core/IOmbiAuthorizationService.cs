using System.Threading.Tasks;

namespace Ombi.Auth.Core
{
    public interface IOmbiAuthorizationService
    {
        Task<bool> HasPermissionAsync(string permissionName);
    }
}