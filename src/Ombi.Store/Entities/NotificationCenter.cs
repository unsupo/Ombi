using Ombi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ombi.Store.Entities
{
    public class NotificationCenter : Entity
    {
        public NotificationType Type { get; set; }
        public string NotificationMessage { get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool Read { get; set; }
    }
}
