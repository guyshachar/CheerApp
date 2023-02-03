using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CorePush.Apple;
using CorePush.Interfaces;
using CorePush.Interfaces.Apple;

[assembly: Dependency(nameof(ApnSender), LoadHint.Default)]
namespace CorePush.Apple
{
    /// <summary>
    /// HTTP2 Apple Push Notification sender
    /// </summary>
    public class ApnSender : IApnSender
    {
        private static readonly ConcurrentDictionary<string, Tuple<string, DateTime>> tokens = new ConcurrentDictionary<string, Tuple<string, DateTime>>();
        private static readonly Dictionary<ApnServerType, string> servers = new Dictionary<ApnServerType, string>
        {
            {ApnServerType.Development, "https://api.development.push.apple.com:443" },
            {ApnServerType.Production, "https://api.push.apple.com:443" }
        };

        private const string apnIdHeader = "apns-id";
        private const int tokenExpiresMinutes = 50;

        private readonly IApnSettings apnSettings;
        private readonly IJsonHelper jsonHelper;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Apple push notification sender constructor
        /// </summary>
        /// <param name="apnSettings">Apple Push Notification settings</param>
        /// <param name="http">HTTP client instance</param>
        public ApnSender(IApnSettings apnSettings, IJsonHelper jsonHelper)
        {
            this.apnSettings = apnSettings ?? throw new ArgumentNullException(nameof(apnSettings));
            this.jsonHelper = jsonHelper;
            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Serialize and send notification to APN. Please see how your message should be formatted here:
        /// https://developer.apple.com/library/archive/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1
        /// Payload will be serialized using Newtonsoft.Json package.
        /// !IMPORTANT: If you send many messages at once, make sure to retry those calls. Apple typically doesn't like 
        /// to receive too many requests and may occasionally respond with HTTP 429. Just try/catch this call and retry as needed.
        /// </summary>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public async Task<ApnsResponse> SendAsync(
            object notification,
            string deviceToken,
            string apnsId = null,
            int apnsExpiration = 0,
            int apnsPriority = 10,
            ApnPushType apnPushType = ApnPushType.Alert,
            ApnServerType apnServerType = ApnServerType.Development,
            CancellationToken cancellationToken = default)
        {
            httpClient.BaseAddress = new Uri(servers[apnServerType]);

            var path = $"/3/device/{deviceToken}";
            var json = jsonHelper.Serialize(notification);

            using (var message = new HttpRequestMessage(HttpMethod.Post, path))
            {
                message.Version = new Version(2, 0);
                message.Content = new StringContent(json);

                message.Headers.Authorization = new AuthenticationHeaderValue("bearer", GetJwtToken());
                message.Headers.TryAddWithoutValidation(":method", "POST");
                message.Headers.TryAddWithoutValidation(":path", path);
                message.Headers.Add("apns-topic", apnSettings.AppBundleIdentifier);
                message.Headers.Add("apns-expiration", apnsExpiration.ToString());
                message.Headers.Add("apns-priority", apnsPriority.ToString());
                message.Headers.Add("apns-push-type", apnPushType.ToString().ToLowerInvariant()); // required for iOS 13+

                if (!string.IsNullOrWhiteSpace(apnsId))
                {
                    message.Headers.Add(apnIdHeader, apnsId);
                }

                using (var response = await httpClient.SendAsync(message, cancellationToken))
                {
                    var succeed = response.IsSuccessStatusCode;
                    var content = await response.Content.ReadAsStringAsync();
                    var error = jsonHelper.Deserialize<ApnsError>(content);

                    return new ApnsResponse
                    {
                        IsSuccess = succeed,
                        Error = error
                    };
                }
            }
        }

        private string GetJwtToken()
        {
            var (token, date) = tokens.GetOrAdd(apnSettings.AppBundleIdentifier, _ => new Tuple<string, DateTime>(CreateJwtToken(), DateTime.UtcNow));
            if (date < DateTime.UtcNow.AddMinutes(-tokenExpiresMinutes))
            {
                tokens.TryRemove(apnSettings.AppBundleIdentifier, out _);
                return GetJwtToken();
            }

            return token;
        }

        public string CreateJwtToken()
        {
            var header = jsonHelper.Serialize(new { alg = "ES256", kid = CleanP8Key(apnSettings.P8PrivateKeyId) });
            var payload = jsonHelper.Serialize(new { iss = apnSettings.TeamId, iat = EpochTime() });
            /*
            var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
            var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            var unsignedJwtData = $"{headerBase64}.{payloadBase64}";
            var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);

            using (var dsa = AppleCryptoHelper.GetEllipticCurveAlgorithm(CleanP8Key(apnSettings.P8PrivateKey)))
            {
                var signature = dsa.SignData(unsignedJwtBytes, 0, unsignedJwtBytes.Length, HashAlgorithmName.SHA256);
                return $"{unsignedJwtData}.{Convert.ToBase64String(signature)}";
            }*/
            var jwt = SignES256(apnSettings.P8PrivateKey, header, payload);
            return jwt;
        }

        /// <summary>
        /// Method returns ECDSA signed JWT token format, from json header, json payload and privateKey (pure string extracted from *.p8 file - PKCS#8 format)
        /// </summary>
        /// <param name="privateKey">ECDSA256 key</param>
        /// <param name="header">JSON header, i.e. "{\"alg\":\"ES256\" ,\"kid\":\"1234567899"\"}"</param>
        /// <param name="payload">JSON payload, i.e.  {\"iss\":\"MMMMMMMMMM"\",\"iat\":"122222222229"}"</param>
        /// <returns>base64url encoded JWT token</returns>
        private string SignES256(string privateKey, string header, string payload)
        {
            try
            {
                var pk = privateKey.Replace("\n", "");
                var a = Convert.FromBase64String(pk);
                var key = CngKey.Import(a, CngKeyBlobFormat.Pkcs8PrivateBlob);

                using (ECDsaCng dsa = new ECDsaCng(key))
                {
                    dsa.HashAlgorithm = CngAlgorithm.Sha256;
                    var unsignedJwtData =
                        Base64UrlEncode(header) + "." + Base64UrlEncode(payload);
                    var signature = dsa.SignData(Encoding.UTF8.GetBytes(unsignedJwtData));
                    return unsignedJwtData + "." + Base64UrlEncode(System.Text.Encoding.UTF8.GetString(signature));
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }

        private static int EpochTime()
        {
            var span = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt32(span.TotalSeconds);
        }

        private static string CleanP8Key(string p8Key)
        {
            // If we have an empty p8Key, then don't bother doing any tasks.
            if (string.IsNullOrEmpty(p8Key))
            {
                return p8Key;
            }

            var lines = p8Key.Split('\n').ToList();

            if (0 != lines.Count && lines[0].StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                lines.RemoveAt(0);
            }

            if (0 != lines.Count && lines[lines.Count - 1].StartsWith("-----END PRIVATE KEY-----"))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            var result = string.Join(string.Empty, lines);

            return result;
        }
    }
}