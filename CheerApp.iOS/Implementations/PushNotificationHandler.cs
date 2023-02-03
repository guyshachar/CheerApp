using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CheerApp;
using CheerApp.iOS.Implementations;
using Plugin.PushNotification;
using DependencyAttribute = Xamarin.Forms.DependencyAttribute;

[assembly: Dependency(typeof(PushNotificationHandler))]
namespace CheerApp.iOS.Implementations
{
    public class PushNotificationHandler : IPushNotificationHandler
    {
        public PushNotificationHandler()
        {
        }

        public void OnAction(NotificationResponse response)
        {
        }

        public void OnError(string error)
        {
        }

        public void OnOpened(NotificationResponse response)
        {
        }

        public void OnReceived(IDictionary<string, object> parameters)
        {
        }
    }
}