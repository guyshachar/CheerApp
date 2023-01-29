using System;

namespace CheerApp.iOS.Services
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
            }
        }
    }
}