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

        [JsonPropertyName("data")]
        public object Data { get; set; }

        public FcmMessage(string fcmToken, string title, string body, object data)
        {
            FcmToken = fcmToken;

            NotificationBody = new Notification
            {
                Title = title,
                Body = body
            };

            Data = data;
        }
    }
}