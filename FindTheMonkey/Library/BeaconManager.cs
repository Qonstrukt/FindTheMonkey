using System;
using System.Collections.Generic;
using System.Linq;

using CoreLocation;
using Foundation;

namespace FindTheMonkey
{
	public static class BeaconManager
	{
		private const string regionId = "fhBeacon";
		private const string uuid = "B9407F30-F5F8-466E-AFF9-25556B57FE6D";

		private static CLRegion lastRegion;
		private static CLLocationManager locationManager;

		public static Dictionary<string, CLBeacon> RangedBeacons { get; private set; }

		public static IBeaconManagerDelegate Delegate { get; set; }


		static BeaconManager ()
		{
			RangedBeacons = new Dictionary<string, CLBeacon> ();

			locationManager = new CLLocationManager ();

			locationManager.RegionEntered += HandleRegionEntered;
			locationManager.DidRangeBeacons += HandleDidRangeBeacons;

			locationManager.DidStartMonitoringForRegion += HandleDidStartMonitoringForRegion;
			locationManager.MonitoringFailed += HandleMonitoringFailed;
			locationManager.RangingBeaconsDidFailForRegion += HandleMonitoringFailed;

			locationManager.RequestAlwaysAuthorization ();

			var region = CreateBeaconRegion ();

			switch (CLLocationManager.Status) {
			case CLAuthorizationStatus.AuthorizedAlways:
				locationManager.StartMonitoring (region);
				locationManager.StartRangingBeacons (region);
				break;

			default:
				HandleMonitoringFailed (null, new EventArgs ());
				break;
			}
		}

		private static CLBeaconRegion CreateBeaconRegion ()
		{
			var NSUUID = new NSUuid (uuid);
			var beaconRegion = new CLBeaconRegion (NSUUID, regionId);

			beaconRegion.NotifyEntryStateOnDisplay = true;
			beaconRegion.NotifyOnEntry = true;
			beaconRegion.NotifyOnExit = true;

			return beaconRegion;
		}

		private static void HandleRegionEntered (object sender, CLRegionEventArgs e)
		{
			Delegate.RegionEnetered (e.Region, lastRegion);

			lastRegion = e.Region;
		}

		private static void HandleDidRangeBeacons (object sender, CLRegionBeaconsRangedEventArgs e)
		{
			var beacons = new Dictionary<string, CLBeacon> ();

			foreach (var beacon in e.Beacons) {
				var id = beacon.Minor.ToString ();

				beacons.Add (id, beacon);

				RangedBeacons = beacons.OrderBy (obj => {
					return obj.Value.Proximity == CLProximity.Unknown ? 4 : (int)obj.Value.Proximity;
				}).ToDictionary (obj => obj.Key, obj => obj.Value);

				Delegate.BeaconsRanged (e.Beacons, e.Region);
			}
		}

		private static void HandleDidStartMonitoringForRegion (object sender, CLRegionEventArgs e)
		{
			Delegate.MonitoringStarted (e.Region);
		}

		private static void HandleMonitoringFailed (object sender, EventArgs e)
		{
			Delegate.MonitoringFailed ();
		}
	}
}

