using System;
using CoreLocation;

namespace FindTheMonkey
{
	public interface IBeaconManagerDelegate
	{
		void BeaconsRanged (CLBeacon[] beacons, CLBeaconRegion region);

		void MonitoringStarted (CLRegion region);

		void MonitoringFailed ();

		void RegionEnetered (CLRegion region, CLRegion previous);
	}
}

