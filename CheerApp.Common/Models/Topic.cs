using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CheerApp.Common.Interfaces;
using Google.Cloud.Firestore;

namespace CheerApp.Common.Models
{
    [FirestoreData]
    public class Topic : ModelBase
    {
        public Topic() : base()
        {
        }

        static Topic()
        {
            properties = GetProperties<Topic>();
        }

        private static PropertyInfo[] properties;
        public override PropertyInfo[] Properties => properties;

        [FirestoreProperty]
        public string Description { get; set; }
        [FirestoreProperty]
        public List<string> DeviceIds { get; set; }

        public static Topic Init(string id)
        {
            var topic = new Topic
            {
                Id = id,
                DeviceIds = new List<string>()
            };
            return topic;
        }
    }
}