using System;
using CoreLocation;

namespace Freshheads.SmartRoom.iOS
{
	public interface IBeaconManagerDelegate
	{
		void BeaconsRanged (CLBeacon[] beacons, CLBeaconRegion region);

		void MonitoringStarted (CLBeaconRegion region);

		void MonitoringFailed ();

		void RegionEnetered (CLBeaconRegion region, CLBeaconRegion previous);

		void RegionLeft (CLBeaconRegion region);
	}
}

