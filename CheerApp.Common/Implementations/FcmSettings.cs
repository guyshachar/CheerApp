using CorePush.Interfaces.Google;

namespace CheerApp.iOS.Implementations
{
    public class FcmSettings : IFcmSettings
    {
        /// <summary>
        /// FCM Sender ID
        /// </summary>
        public string SenderId => "569111967079";

        /// <summary>
        /// FCM Server Key
        /// </summary>
        public string ServerKey => "AAAAhIG3dWc:APA91bGI0c6gDCbvnr9gS8HmITM8QqDuUHZ5RfQ51FnzCgVo9gevv8jcTWAJwUUQaXRHPCMqbw8i6ZhNIymIGT5l2k5MlvNBhZuW9Yik1TrxZf4HCmIWysufSkiGeyV3hFo6AX2_ENku";
    }
}