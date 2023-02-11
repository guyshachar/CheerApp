using System;

namespace CheerApp.Common.Models
{
    public class ShowRoomNotification : CheerAppNotification
    {
        public ShowRoomNotification() : base()
        {
        }

        public ShowRoomNotification(Message message) :
            base(message.Json)
        {
        }

        public string Action
        {
            get { return GetValue<string>(nameof(Action)); }
            set { Set(nameof(Action), value); }
        }

        public DateTime StartTime
        {
            get { return DateTime.Parse(GetValue<string>(nameof(StartTime))).ToUniversalTime(); }
            set { Set(nameof(StartTime), value.ToString("u")); }
        }

        public string Json
        {
            get { return GetValue<string>(nameof(Json)); }
            set { Set(nameof(Json), value); }
        }
    }
}