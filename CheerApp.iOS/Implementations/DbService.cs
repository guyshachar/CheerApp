using System;
using CheerApp.iOS.Implementations;
using CheerApp.iOS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using CorePush.Interfaces;
using CheerApp.iOS;
using CheerApp.iOS.Interfaces;

namespace CheerApp.iOS.Implementations
{
    public class DbService : IDbService
    {
        private readonly IRepository<DeviceDetails> DeviceDetailsRepository;
        private readonly IRepository<Topic> TopicRepository;

        public DbService(
            IRepository<DeviceDetails> deviceDetailsRepository,
            IRepository<Topic> topicRepository)
        {
            DeviceDetailsRepository = deviceDetailsRepository;
            TopicRepository = topicRepository;
        }

        public async Task SendDeviceDetailsToServerAsync(string apnToken = null, string fcmToken = null, string topicIds = null)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(DbService)}:{nameof(SendDeviceDetailsToServerAsync)} apnToken={apnToken} fcmToken={fcmToken} topicId={topicIds}");

            var deviceDetails = await DeviceDetailsRepository.GetAsync(Startup.DeviceId) ?? DeviceDetails.Init(Startup.DeviceId);

            deviceDetails.Name = UIDevice.CurrentDevice.Name;
            deviceDetails.Description = UIDevice.CurrentDevice.Description;
            deviceDetails.Version = UIDevice.CurrentDevice.SystemVersion;

            if (apnToken != null)
                deviceDetails.ApnToken = apnToken;

            if (fcmToken != null)
                deviceDetails.FcmToken = fcmToken;

            var topicsChanged = false;
            var existingTopicIds = deviceDetails.Topics.Split(',').Where(topic => !string.IsNullOrEmpty(topic)).ToList();
            if (topicIds != null && deviceDetails.Topics != topicIds)
            {
                topicsChanged = true;
                deviceDetails.Topics = topicIds;
            }
            deviceDetails.UpdateDateTime = DateTime.UtcNow.ToString();
            await DeviceDetailsRepository.AddUpdateAsync(deviceDetails.Id, deviceDetails);

            if (!topicsChanged)
                return;

            var newTopicIds = topicIds.Split(',').Where(topic => !string.IsNullOrEmpty(topic)).ToList();
            var topicsToAdd = newTopicIds.Except(existingTopicIds).Union(new List<string> { Startup.TOPIC_ALL });
            var topicsToRemove = existingTopicIds.Except(newTopicIds);

            foreach (var topicId in topicsToAdd)
            {
                var topic = await TopicRepository.GetAsync(topicId);
                if (topic == null)
                    topic = Topic.Init(topicId);
                var deviceIds = (topic.DeviceIds ?? string.Empty).Split(',').ToList();
                if (deviceIds.Contains(deviceDetails.Id))
                    continue;

                deviceIds.Add(deviceDetails.Id);
                topic.DeviceIds = string.Join(',', deviceIds);
                topic.UpdateDateTime = DateTime.UtcNow.ToString();
                await TopicRepository.AddUpdateAsync(topicId, topic);
            }

            foreach (var topicId in topicsToRemove)
            {
                if (topicId == Startup.TOPIC_ALL)
                    continue;
                var topic = await TopicRepository.GetAsync(topicId);
                if (topic == null)
                    continue;
                var deviceIds = (topic.DeviceIds ?? string.Empty).Split(',').ToList();
                if (!deviceIds.Contains(deviceDetails.Id))
                    continue;

                deviceIds.Remove(deviceDetails.Id);
                topic.DeviceIds = string.Join(',', deviceIds);
                topic.UpdateDateTime = DateTime.UtcNow.ToString();
                await TopicRepository.AddUpdateAsync(topicId, topic);
            }
        }
    }
}