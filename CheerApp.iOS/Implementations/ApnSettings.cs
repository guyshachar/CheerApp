using CheerApp.iOS.Implementations;
using CorePush.Interfaces.Apple;
using DependencyAttribute = Xamarin.Forms.DependencyAttribute;

[assembly: Dependency(typeof(ApnSettings))]
namespace CheerApp.iOS.Implementations
{
    public class ApnSettings : IApnSettings
    {
        /// <summary>
        /// p8 certificate string
        /// </summary>
        public string P8PrivateKey => "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQg08h/581SUupD9s0U\ncd0nqyl0ouhZlhc3jzu1Po7Wbm6gCgYIKoZIzj0DAQehRANCAAQvk4SdvsRJTJ7g\nck+eQcpsaMo7cc5Xd+pgR+DdAz3At/i+1P6TQckIMjLXoTt+tSbYlgtEaCuf5G/7\n0sNoqmD1";

        /// <summary>
        /// 10 digit p8 certificate id. Usually a part of a downloadable certificate filename
        /// </summary>
        public string P8PrivateKeyId => "XW56FZN75L";

        /// <summary>
        /// Apple 10 digit team id
        /// </summary>
        public string TeamId => "BLLS9LPHDG";

        /// <summary>
        /// App slug / bundle name
        /// </summary>
        public string AppBundleIdentifier => "com.guyshachar.cheerapp";
    }
}