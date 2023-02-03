using System;
using System.Text.Json.Serialization;

namespace CorePush.Google
{
    public class FcmMessage
    {
        [JsonPropertyName("to")]
        public string FcmToken { get; set; }

        public class Notification
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("body")]
            public string Body { get; set; }
        }

        [JsonPropertyName("notification")]
        public Notification NotificationBody { get; set; }

        public class Data
        {
            [JsonPropertyName("nick")]
            public string Nick { get; set; }

            [JsonPropertyName("room")]
            public string Room { get; set; }
        }

        [JsonPropertyName("data")]
        public Data DataBody { get; set; }

        public FcmMessage(string fcmToken, string title, string body, string nick = "", string room = "")
        {
            FcmToken = fcmToken;

            NotificationBody = new Notification
            {
                Title = title,
                Body = body
            };

            DataBody = new Data
            {
                Nick = nick,
                Room = room
            };
        }
    }
}