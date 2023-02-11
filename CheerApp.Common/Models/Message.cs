using System;
using System.Reflection;
using Google.Cloud.Firestore;

namespace CheerApp.Common.Models
{
    [FirestoreData]
    public class Message : ModelBase
    {
        public Message() : base()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        static Message()
        {
            properties = GetProperties<Message>();
        }

        private static PropertyInfo[] properties;
        public override PropertyInfo[] Properties => properties;

        [FirestoreProperty]
        public string Title { get; set; }
        [FirestoreProperty]
        public string Page { get; set; }
        [FirestoreProperty]
        public string Action { get; set; }
        [FirestoreProperty]
        public string StartTime { get; set; }
        [FirestoreProperty]
        public string Json { get; set; }
    }
}