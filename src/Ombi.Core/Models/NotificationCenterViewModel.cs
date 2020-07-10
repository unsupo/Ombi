using Ombi.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Core.Models
{
    public class NotificationViewModel
    {
        public int NotificaionId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserId { get; set; }
        public NotificationType Type { get; set; }
        public bool Read { get; set; }
    }
    public class NotificationCenterViewModel
    {
        public int UnreadCount { get; set; }
        public List<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();
    }
}
