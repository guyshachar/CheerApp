using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Foundation;

namespace CheerApp.iOS.Models
{
    public class Topic : IModel<Topic>
    {
        public Topic()
        {
        }

        public string Id { get; set; }

        public string Description { get; set; }

        public string DeviceIds { get; set; }

        public string UpdateDateTime { get; set; }

        public static Topic Init(string id)
        {
            var topic = new Topic
            {
                Id = id,
            };
            return topic;
        }

        public async Task<bool> Doc2ObjAsync(DocumentReference docRef)
        {
            var docSnapshot = await docRef.GetDocumentAsync();
            var data = docSnapshot.Data;

            if (docSnapshot.Exists)
            {
                Id = data[nameof(Topic.Id)].ToString();
                Description = data[nameof(Topic.Description)].ToString();
                DeviceIds = data[nameof(Topic.DeviceIds)].ToString();
                UpdateDateTime = data[nameof(DeviceDetails.UpdateDateTime)].ToString();
            }

            return docSnapshot.Exists;
        }

        public async Task Obj2DocAsync(DocumentReference docRef)
        {
            var dataDict = new Dictionary<object, object>
            {
                {(object)nameof(Topic.Id), Id},
                {(object)nameof(Topic.Description), Description},
                {(object)nameof(Topic.DeviceIds), DeviceIds},
                {(object)nameof(Topic.UpdateDateTime), UpdateDateTime},
            };
            await docRef.SetDataAsync(dataDict);
        }
    }
}