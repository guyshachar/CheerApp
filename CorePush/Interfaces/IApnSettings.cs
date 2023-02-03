namespace CorePush.Interfaces.Apple
{
    public interface IApnSettings
    {
        /// <summary>
        /// p8 certificate string
        /// </summary>
        string P8PrivateKey { get; }

        /// <summary>
        /// 10 digit p8 certificate id. Usually a part of a downloadable certificate filename
        /// </summary>
        string P8PrivateKeyId { get; }

        /// <summary>
        /// Apple 10 digit team id
        /// </summary>
        string TeamId { get; }

        /// <summary>
        /// App slug / bundle name
        /// </summary>
        string AppBundleIdentifier { get; }
    }
}