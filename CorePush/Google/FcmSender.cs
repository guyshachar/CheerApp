using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CorePush.Google;
using CorePush.Interfaces.Google;
using CorePush.Utils;
using Xamarin.Forms;

[assembly: Dependency(typeof(FcmSender))]
namespace CorePush.Google
{
    /// <summary>
    /// Firebase message sender
    /// </summary>
    public class FcmSender : IFcmSender
    {
        private const string fcmUrl = "https://fcm.googleapis.com/fcm/send";

        private readonly IFcmSettings settings;
        private readonly HttpClient httpClient;

        public FcmSender(IFcmSettings fcmSettings)
        {
            this.settings = fcmSettings;
            this.httpClient = new HttpClient();

            if (httpClient.BaseAddress == null)
            {
                httpClient.BaseAddress = new Uri(fcmUrl);
            }
        }

        /// <summary>
        /// Send firebase notification.
        /// Please check out payload formats:
        /// https://firebase.google.com/docs/cloud-messaging/concept-options#notifications
        /// The SendAsync method will add/replace "to" value with deviceId
        /// </summary>
        /// <param name="deviceId">Device token (will add `to` to the payload)</param>
        /// <param name="payload">Notification payload that will be serialized using Newtonsoft.Json package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public Task<FcmResponse> SendAsync(string deviceId, object payload, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            {
                using (var utf8JsonWriter1 = new Utf8JsonWriter(ms))
                {
                    //For each level in json tree an additional dictionary must be added
                    var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(payload));
                    jsonDict.Remove("to");
                    jsonDict.Add("to", deviceId);
                    JsonSerializer.Serialize<object>(utf8JsonWriter1, jsonDict);

                    return SendAsync(jsonDict, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Send firebase notification.
        /// Please check out payload formats:
        /// https://firebase.google.com/docs/cloud-messaging/concept-options#notifications
        /// The SendAsync method will add/replace "to" value with deviceId
        /// </summary>
        /// <param name="payload">Notification payload that will be serialized using Newtonsoft.Json package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public async Task<FcmResponse> SendAsync(object payload, CancellationToken cancellationToken = default)
        {
            var jsonHelper = new JsonHelper();
            var serialized = jsonHelper.Serialize(payload);

            using (var message = new HttpRequestMessage())
            {
                message.Method = HttpMethod.Post;
                message.Headers.Add("Authorization", $"key = {settings.ServerKey}");

                if (!string.IsNullOrEmpty(settings.SenderId))
                {
                    message.Headers.Add("Sender", $"id = {settings.SenderId}");
                }

                message.Content = new StringContent(serialized, Encoding.UTF8, "application/json");

                try
                {
                    using (var response = await httpClient.SendAsync(message, cancellationToken))
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Firebase notification error: " + responseString);
                        }

                        return jsonHelper.Deserialize<FcmResponse>(responseString);
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
    }
}