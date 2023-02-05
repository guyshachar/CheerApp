using System.Collections.Generic;
using CorePush.Google;

namespace CheerApp.Common.Models
{
    public class CheerAppNotification : FcmNotificationBase
    {
        public CheerAppNotification() : base()
        {
        }

        public CheerAppNotification(IDictionary<string, object> notificationData = null) : base(notificationData)
        {
        }

        public string ScreenName
        {
            get { return GetValue<string>(nameof(ScreenName)); }
            set { Set(nameof(ScreenName), value); }
        }
    }
}