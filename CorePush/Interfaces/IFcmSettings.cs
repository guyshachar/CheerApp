namespace CorePush.Google
{
    public interface IFcmSettings
    {
        string SenderId { get; }
        string ServerKey { get; }
    }
}