using CheerApp.iOS.Implementations;
using CheerApp.iOS.Models;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceDetailsRepository))]
namespace CheerApp.iOS.Implementations
{
	public class DeviceDetailsRepository : Repository<DeviceDetails>
	{
		public DeviceDetailsRepository() : base("CheerAppDevices")
		{
		}
	}
}