using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core
{
    public class NotificationCenterService
    {
        private readonly IRepository<NotificationCenter> _repository;
        private readonly IPrincipal _user;
        private readonly OmbiUserManager _userManager;

        public NotificationCenterService(IRepository<NotificationCenter> repository, IPrincipal user, OmbiUserManager userManager)
        {
            _repository = repository;
            _user = user;
            _userManager = userManager;
        }

        public async Task<NotificationCenterViewModel> GetNoficiations()
        {
            // Hard code to 30 for now?
            var user = await GetUser();
            var notifications = _repository.GetAll().Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedOn).Take(30);

            var vm = new NotificationCenterViewModel();
            foreach (var n in notifications)
            {
                vm.Notifications.Add(new NotificationViewModel
                {
                    CreatedOn = n.CreatedOn,
                    NotificaionId = n.Id,
                    Read = n.Read,
                    Type = n.Type,
                     UserId = n.UserId
                });
            }

            vm.UnreadCount = vm.Notifications.Count(x => !x.Read);
            return vm; 
        }

        public async Task ReadNotification(int notificationId)
        {
            var user = await GetUser();
            var notifications = await _repository.GetAll().FirstOrDefaultAsync(x => x.UserId == user.Id && x.Id == notificationId);
            if(notifications == null)
            {
                return;
            }

            notifications.Read = true;
            await _repository.SaveChangesAsync();
        }

        protected string Username => _user.Identity.Name;

        protected async Task<OmbiUser> GetUser()
        {
            var username = Username.ToUpper();
            return await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
        }
    }
}
