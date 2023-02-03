using System;

namespace CheerApp.iOS.Implementations
{
    public static class FirestoreService
    {
        public static Firebase.CloudFirestore.Firestore Instance => Firebase.CloudFirestore.Firestore.SharedInstance;

        public static void Init()
        {
            try
            {
                Firebase.Core.App.Configure();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(FirestoreService)}:{nameof(Init)} Exception={e.Message} {e.InnerException}");
            }
        }
    }
}