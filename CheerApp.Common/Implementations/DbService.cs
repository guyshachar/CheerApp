using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;

namespace CheerApp.Common.Implementations
{
    public class DbService : IDbService
    {
        private readonly IFirestoreProvider _firestoreProvider;
        private readonly IDeviceHelper _deviceHelper;

        public DbService(IFirestoreProvider firestoreProvider, IDeviceHelper deviceHelper)
        {
            _firestoreProvider = firestoreProvider;
            _deviceHelper = deviceHelper;
        }

        public async Task SendDeviceDetailsToServerAsync(string apnToken = null, string fcmToken = null, string topicIds = null)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(DbService)}:{nameof(SendDeviceDetailsToServerAsync)} apnToken={apnToken} fcmToken={fcmToken} topicId={topicIds}");

            var deviceDetail = await _firestoreProvider.GetAsync<DeviceDetail>(Startup.DeviceId) ?? DeviceDetail.Init(Startup.DeviceId);

            _deviceHelper.FillDeviceDetails(deviceDetail);

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
            await _firestoreProvider.AddUpdateAsync(deviceDetail);

            if (!topicsChanged)
                return;

            var newTopicIds = topicIds.Split(',').Where(topic => !string.IsNullOrEmpty(topic)).ToList();
            var topicsToAdd = newTopicIds.Except(existingTopicIds).Union(new List<string> { Startup.TOPIC_ALL });
            var topicsToRemove = existingTopicIds.Except(newTopicIds);

            foreach (var topicId in topicsToAdd)
            {
                var topic = await _firestoreProvider.GetAsync<Topic>(topicId);
                if (topic == null)
                    topic = Topic.Init(topicId);
                var deviceIds = topic.DeviceIds;
                if (deviceIds.Contains(deviceDetail.Id))
                    continue;

                deviceIds.Add(deviceDetail.Id);
                topic.DeviceIds = deviceIds;
                await _firestoreProvider.AddUpdateAsync(topic);
            }

            foreach (var topicId in topicsToRemove)
            {
                if (topicId == Startup.TOPIC_ALL)
                    continue;
                var topic = await _firestoreProvider.GetAsync<Topic>(topicId);
                if (topic == null)
                    continue;
                var deviceIds = topic.DeviceIds;
                if (!deviceIds.Contains(deviceDetail.Id))
                    continue;

                deviceIds.Remove(deviceDetail.Id);
                topic.DeviceIds = deviceIds;
                await _firestoreProvider.AddUpdateAsync(topic);
            }
        }
    }
}