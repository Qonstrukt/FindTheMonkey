using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FindTheMonkey
{
	public static class BeaconManager
	{
		private const string regionId = "fhBeacon";
		private const string uuid = "B9407F30-F5F8-466E-AFF9-25556B57FE6D";

		private static NSNumber currentBeaconMinor;
		private static NSNumber previousBeaconMinor;
		private static CLProximity currentBeaconProximity;
		private static CLProximity previousBeaconProximity;

		private static CLLocationManager locationManager;
		private static BeaconRestClient restClient;

		public static readonly Dictionary<string, EstimoteLocation> Beacons;
		public static readonly Dictionary<string, CLProximity> Proximities;

		public static EstimoteLocation CurrentLocation { get; private set; }


		public static event EventHandler<int> BeaconUpdated = delegate {};
		public static event EventHandler BeaconsUpdated = delegate {};
		public static event EventHandler MonitoringStarted = delegate {};
		public static event EventHandler MonitoringFailed = delegate {};


		static BeaconManager ()
		{
			Beacons = new Dictionary<string, EstimoteLocation> ();
			Proximities = new Dictionary<string, CLProximity> ();

			locationManager = new CLLocationManager ();
			restClient = BeaconRestClient.SharedInstance;
		}

		public static void Initialize ()
		{
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

		private static async void UpdateBeaconInfo (string id)
		{
			try {
				Beacons [id] = await restClient.GetEstimote (id);

				var index = Beacons.Keys.ToList ().IndexOf (id);
				BeaconUpdated.Invoke (null, index);
			} catch (Exception) {
				// Silently fail for now
			}
		}

		private static void HandleRegionEntered (object sender, CLRegionEventArgs e)
		{
			if (e.Region.Identifier == regionId) {
				var notification = new UILocalNotification { AlertBody = "Welkom bij Freshheads!" };
				UIApplication.SharedApplication.PresentLocationNotificationNow (notification);
			}
		}

		private static void HandleDidRangeBeacons (object sender, CLRegionBeaconsRangedEventArgs e)
		{
			foreach (var beacon in e.Beacons) {
				var id = beacon.Minor.ToString ();

				if (!Beacons.ContainsKey (id)) {
					Beacons.Add (id, new EstimoteLocation {
						Department = String.Format ("Beacon {0}", beacon.Minor)
					});
					Proximities.Add (id, beacon.Proximity);

					UpdateBeaconInfo (id);
				} else {
					Proximities [id] = beacon.Proximity;
				}

				if (currentBeaconMinor != null && beacon.Minor == previousBeaconMinor && (beacon.Proximity == CLProximity.Far || previousBeaconProximity == CLProximity.Unknown)) {
					// We're monitoring a new beacon, and the previous beacon is now out of range
					CurrentLocation = Beacons [currentBeaconMinor.ToString ()];

					previousBeaconMinor = currentBeaconMinor;
					previousBeaconProximity = currentBeaconProximity;
					currentBeaconMinor = null;
					currentBeaconProximity = CLProximity.Unknown;
				} else if (beacon.Minor != previousBeaconMinor && (beacon.Proximity == CLProximity.Near || beacon.Proximity == CLProximity.Immediate)) {
					// We're seeing a new beacon within close proximity
					if (previousBeaconProximity == CLProximity.Far || previousBeaconProximity == CLProximity.Unknown) {
						// The previous beacon is now out of reach, the current beacon will become the new previous beacon
						CurrentLocation = Beacons [beacon.Minor.ToString ()];

						previousBeaconMinor = beacon.Minor;
						previousBeaconProximity = beacon.Proximity;
						currentBeaconMinor = null;
						currentBeaconProximity = CLProximity.Unknown;
					} else {
						// The prvious beacon isn't that far though, so save this new one for later
						currentBeaconMinor = beacon.Minor;
						currentBeaconProximity = beacon.Proximity;
					}
				} else if (previousBeaconMinor == beacon.Minor) {
					previousBeaconProximity = beacon.Proximity;
				}

				BeaconsUpdated.Invoke (null, new EventArgs ());
			}
		}

		private static void HandleDidStartMonitoringForRegion (object sender, CLRegionEventArgs e)
		{
			MonitoringStarted.Invoke (null, new EventArgs ());
		}

		private static void HandleMonitoringFailed (object sender, EventArgs e)
		{
			MonitoringFailed.Invoke (null, new EventArgs ());
		}
	}
}

