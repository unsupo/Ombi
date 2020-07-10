using Ombi.Core.Models;
using System.Threading.Tasks;

namespace Ombi.Core
{
    public interface INotificationCenterService
    {
        Task<NotificationCenterViewModel> GetNoficiations();
        Task ReadNotification(int notificationId);
    }
}
