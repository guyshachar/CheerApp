using System;

namespace CheerApp.Common
{
	public class Notification
	{
		public string ScreenName { get; set; }
		public string Action { get; set; }
		public DateTime StartTime { get; set; }
		public string Json { get; set; }
	}
}