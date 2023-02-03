using UIKit;

namespace CheerApp.iOS
{
    public class Application
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Application)} {nameof(Main)} Start...");
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}