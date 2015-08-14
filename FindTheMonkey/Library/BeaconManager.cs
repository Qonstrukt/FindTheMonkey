using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Foundation;

namespace Freshheads.SmartRoom.iOS
{
	public static class BeaconManager
	{
		private static List<CLBeaconRegion> beaconRegions;
		private static CLBeaconRegion currentRegion;
		private static CLLocationManager locationManager;

		public static IBeaconManagerDelegate Delegate { get; set; }


		static BeaconManager ()
		{
			beaconRegions = new List<CLBeaconRegion> ();
			locationManager = new CLLocationManager ();

			locationManager.RegionEntered += HandleRegionEntered;
			locationManager.RegionLeft += HandleRegionLeft;
			locationManager.DidRangeBeacons += HandleDidRangeBeacons;
			locationManager.DidStartMonitoringForRegion += HandleDidStartMonitoringForRegion;
			locationManager.MonitoringFailed += HandleMonitoringFailed;
			locationManager.RangingBeaconsDidFailForRegion += HandleMonitoringFailed;

			locationManager.RequestAlwaysAuthorization ();

			if (CLLocationManager.Status == CLAuthorizationStatus.Denied) {
				HandleMonitoringFailed (null, new EventArgs ());
			}

			#if DEBUG
//			RegisterBeaconRegion ("B9407F30-F5F8-466E-AFF9-25556B57FE6D", "Estimotes");
//			RegisterBeaconRegion ("8492E75F-4FD6-469D-B132-043FE94921D8", "PhoneEstimotes");
			#endif
		}

		public static void RegisterBeaconRegion (string uuid, ushort major, string regionId)
		{
			var NSUUID = new NSUuid (uuid);
			var beaconRegion = new CLBeaconRegion (NSUUID, major, regionId);

			beaconRegion.NotifyEntryStateOnDisplay = true;
			beaconRegion.NotifyOnEntry = true;
			beaconRegion.NotifyOnExit = true;

			locationManager.StartMonitoring (beaconRegion);
			locationManager.StartRangingBeacons (beaconRegion);

			beaconRegions.Add (beaconRegion);
		}

		private static void HandleRegionEntered (object sender, CLRegionEventArgs e)
		{
			var beaconRegion = e.Region as CLBeaconRegion;

			Delegate.RegionEnetered (beaconRegion, currentRegion);

			currentRegion = beaconRegion;
		}

		static void HandleRegionLeft (object sender, CLRegionEventArgs e)
		{
			var beaconRegion = e.Region as CLBeaconRegion;

			if (currentRegion == null || currentRegion.Identifier != beaconRegion.Identifier)
				return;
			
			Delegate.RegionLeft (beaconRegion);

			currentRegion = null;
		}

		private static void HandleDidRangeBeacons (object sender, CLRegionBeaconsRangedEventArgs e)
		{
			var beacons = e.Beacons.OrderBy (OrderByProximity).ToArray ();

			Delegate.BeaconsRanged (beacons, e.Region);
		}

		private static void HandleDidStartMonitoringForRegion (object sender, CLRegionEventArgs e)
		{
			var beaconRegion = e.Region as CLBeaconRegion;

			Delegate.MonitoringStarted (beaconRegion);
		}

		private static void HandleMonitoringFailed (object sender, EventArgs e)
		{
			Delegate.MonitoringFailed ();
		}

		public static int OrderByProximity (CLBeacon obj)
		{
			return obj.Proximity == CLProximity.Unknown ? 4 : (int)obj.Proximity;
		}
	}
}

