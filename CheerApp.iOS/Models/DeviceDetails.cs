using System.Collections.Generic;
using System.Threading.Tasks;
using CheerApp.iOS.Models;
using Firebase.CloudFirestore;
using Xamarin.Forms;

namespace CheerApp.iOS.Models
{
    public class DeviceDetails : IModel<DeviceDetails>
    {
        public DeviceDetails()
        { }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string ApnToken { get; set; }
        public string FcmToken { get; set; }
        public string Topics { get; set; }
        public string UpdateDateTime { get; set; }

        public static DeviceDetails Init(string id)
        {
            var deviceDetails = new DeviceDetails
            {
                Id = id,
                Topics = "ALL"
            };

            return deviceDetails;
        }

        public async Task<bool> Doc2ObjAsync(DocumentReference docRef)
        {
            var docSnapshot = await docRef.GetDocumentAsync();
            var data = docSnapshot.Data;

            if (docSnapshot.Exists)
            {
                Id = data[nameof(DeviceDetails.Id)].ToString();
                Name = data[nameof(DeviceDetails.Name)].ToString();
                Description = data[nameof(DeviceDetails.Description)].ToString();
                Version = data[nameof(DeviceDetails.Version)].ToString();
                ApnToken = data[nameof(DeviceDetails.ApnToken)].ToString();
                FcmToken = data[nameof(DeviceDetails.FcmToken)].ToString();
                Topics = data[nameof(DeviceDetails.Topics)].ToString() ?? "ALL";
                UpdateDateTime = data[nameof(DeviceDetails.UpdateDateTime)].ToString();
            }

            return docSnapshot.Exists;
        }

        public async Task Obj2DocAsync(DocumentReference docRef)
        {
            var dataDict = new Dictionary<object, object>
            {
                {(object)nameof(DeviceDetails.Id), Id},
                {(object)nameof(DeviceDetails.Name), Name},
                {(object)nameof(DeviceDetails.Description), Description},
                {(object)nameof(DeviceDetails.Version), Version},
                {(object)nameof(DeviceDetails.ApnToken), ApnToken},
                {(object)nameof(DeviceDetails.FcmToken), FcmToken},
                {(object)nameof(DeviceDetails.Topics), Topics},
                {(object)nameof(DeviceDetails.UpdateDateTime), UpdateDateTime},
            };
            await docRef.SetDataAsync(dataDict);
        }

        public void Copy(DeviceDetails deviceDetails)
        {
            if (deviceDetails == null)
                return;
            Id = deviceDetails.Id;
            Name = deviceDetails.Name;
            Description = deviceDetails.Description;
            Version = deviceDetails.Version;
            ApnToken = deviceDetails.ApnToken;
            FcmToken = deviceDetails.FcmToken;
            Topics = deviceDetails.Topics;
            UpdateDateTime = deviceDetails.UpdateDateTime;
        }
    }
}