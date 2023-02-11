using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using UIKit;

namespace CheerApp.Common.Implementations
{
    public class DbService : IDbService
    {
        private readonly IFirestoreProvider FirestoreProvider;

        public DbService(IFirestoreProvider firestoreProvider)
        {
            FirestoreProvider = firestoreProvider;
        }

        public async Task SendDeviceDetailsToServerAsync(string apnToken = null, string fcmToken = null, string topicIds = null)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(DbService)}:{nameof(SendDeviceDetailsToServerAsync)} apnToken={apnToken} fcmToken={fcmToken} topicId={topicIds}");

            var deviceDetail = await FirestoreProvider.GetAsync<DeviceDetail>(Startup.DeviceId) ?? DeviceDetail.Init(Startup.DeviceId);

            deviceDetail.Name = UIDevice.CurrentDevice.Name;
            deviceDetail.Description = UIDevice.CurrentDevice.Description;
            deviceDetail.Version = UIDevice.CurrentDevice.SystemVersion;

            if (apnToken != null)
                deviceDetail.ApnToken = apnToken;

            if (fcmToken != null)
                deviceDetail.FcmToken = fcmToken;

            var topicsChanged = false;
            var existingTopicIds = deviceDetail.Topics.Where(topic => !string.IsNullOrEmpty(topic)).ToList();
            if (topicIds != null && string.Join(",",deviceDetail.Topics) != topicIds)
            {
                topicsChanged = true;
                deviceDetail.Topics = topicIds.Split(',').ToList();
            }
            await FirestoreProvider.AddUpdateAsync(deviceDetail);

            if (!topicsChanged)
                return;

            var newTopicIds = topicIds.Split(',').Where(topic => !string.IsNullOrEmpty(topic)).ToList();
            var topicsToAdd = newTopicIds.Except(existingTopicIds).Union(new List<string> { Startup.TOPIC_ALL });
            var topicsToRemove = existingTopicIds.Except(newTopicIds);

            foreach (var topicId in topicsToAdd)
            {
                var topic = await FirestoreProvider.GetAsync<Topic>(topicId);
                if (topic == null)
                    topic = Topic.Init(topicId);
                var deviceIds = topic.DeviceIds;
                if (deviceIds.Contains(deviceDetail.Id))
                    continue;

                deviceIds.Add(deviceDetail.Id);
                topic.DeviceIds = deviceIds;
                await FirestoreProvider.AddUpdateAsync(topic);
            }

            foreach (var topicId in topicsToRemove)
            {
                if (topicId == Startup.TOPIC_ALL)
                    continue;
                var topic = await FirestoreProvider.GetAsync<Topic>(topicId);
                if (topic == null)
                    continue;
                var deviceIds = topic.DeviceIds;
                if (!deviceIds.Contains(deviceDetail.Id))
                    continue;

                deviceIds.Remove(deviceDetail.Id);
                topic.DeviceIds = deviceIds;
                await FirestoreProvider.AddUpdateAsync(topic);
            }
        }
    }
}