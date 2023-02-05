using System;
using System.Collections.Generic;

namespace CorePush.Google
{
	public abstract class FcmNotificationBase
	{
		public const string APN_BODY = "aps.alert.body";
		public const string APN_TITLE = "aps.alert.title";

		public IDictionary<string, object> NotificationData;

		public FcmNotificationBase()
		{
			NotificationData = new Dictionary<string, object>();
		}

		public FcmNotificationBase(IDictionary<string, object> notificationData)
		{
			NotificationData = notificationData;
		}

		public object GetValue(string key)
		{
			if (!NotificationData.ContainsKey(key))
				return null;
			return NotificationData[key];
		}

		public T GetValue<T>(string key)
		{
			return (T)GetValue(key);
		}

		public void Set(string key, object value)
		{
			if (!NotificationData.ContainsKey(key))
				NotificationData.Add(key, value);
			else
				NotificationData[key] = value;
		}

		public string Title => GetValue<string>(APN_TITLE);

		public string Body => GetValue<string>(APN_BODY);
	}
}