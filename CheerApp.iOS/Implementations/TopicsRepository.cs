using System;
using CheerApp.iOS.Implementations;
using CheerApp.iOS.Models;
using Xamarin.Forms;

[assembly: Dependency(typeof(TopicRepository))]
namespace CheerApp.iOS.Implementations
{
	public class TopicRepository : Repository<Topic>
	{
		public TopicRepository() : base("CheerAppTopics")
		{
		}
	}
}