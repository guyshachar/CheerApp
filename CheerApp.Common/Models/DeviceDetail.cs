using System.Collections.Generic;
using System.Reflection;
using CheerApp.Common.Interfaces;
using Google.Cloud.Firestore;

namespace CheerApp.Common.Models
{
    [FirestoreData]
    public class DeviceDetail : ModelBase
    {
        public DeviceDetail() : base()
        {
        }

        static DeviceDetail()
        {
            properties = GetProperties<DeviceDetail>();
        }

        private static PropertyInfo[] properties;
        public override PropertyInfo[] Properties => properties;

        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public string Description { get; set; }
        [FirestoreProperty]
        public string Version { get; set; }
        [FirestoreProperty]
        public string ApnToken { get; set; }
        [FirestoreProperty]
        public string FcmToken { get; set; }
        [FirestoreProperty]
        public List<string> Topics { get; set; }
        [FirestoreProperty]
        public List<string> MessageIds { get; set; }

        public static DeviceDetail Init(string id)
        {
            var deviceDetail = new DeviceDetail
            {
                Id = id,
                Topics = new List<string> { "ALL" },
                MessageIds = new List<string>()
            };

            return deviceDetail;
        }
    }
}