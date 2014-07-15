using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;

namespace FindTheMonkey
{
	public partial class BeaconsTableViewController : UITableViewController
	{
		const string UUID = "B9407F30-F5F8-466E-AFF9-25556B57FE6D";
		const string MajorId = "5041";
		const string RegionId = "fhBeacon";

		private EstimoteAPI API;
		private CLLocationManager locationMgr;

		private NSNumber currentBeaconMinor;
		private NSNumber previousBeaconMinor;
		private CLProximity currentBeaconProximity;
		private CLProximity previousBeaconProximity;

		private readonly Dictionary<string, string> Beacons;
		private readonly Dictionary<string, CLProximity> Proximities;

		private string _selectedBeacon;

		private string SelectedBeacon {
			get { return _selectedBeacon; }
			set {
				_selectedBeacon = value;
				ReloadNavigationTitle ();
			}
		}


		public BeaconsTableViewController (IntPtr handle) : base (handle)
		{
			Beacons = new Dictionary<string, string> ();
			Proximities = new Dictionary<string, CLProximity> ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			API = EstimoteAPI.SharedInstance;

			var NSUUID = new NSUuid (UUID);
			var beaconRegion = new CLBeaconRegion (NSUUID, RegionId);

			beaconRegion.NotifyEntryStateOnDisplay = true;
			beaconRegion.NotifyOnEntry = true;
			beaconRegion.NotifyOnExit = true;

			locationMgr = new CLLocationManager ();

			locationMgr.RegionEntered += HandleRegionEntered;
			locationMgr.DidRangeBeacons += HandleDidRangeBeacons;

			locationMgr.DidStartMonitoringForRegion += HandleDidStartMonitoringForRegion;
			locationMgr.MonitoringFailed += HandleMonitoringFailed;
			locationMgr.RangingBeaconsDidFailForRegion += HandleMonitoringFailed;

			locationMgr.StartMonitoring (beaconRegion);
			locationMgr.StartRangingBeacons (beaconRegion);
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return Beacons.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("BeaconCell");

			var beaconLabel = cell.ViewWithTag (1) as UILabel;
			var beaconProximity = cell.ViewWithTag (2) as UISlider;

			var beacon = Beacons.ElementAt (indexPath.Row);
			var proximity = Proximities.ElementAt (indexPath.Row);

			beaconLabel.Text = beacon.Value;

			beaconProximity.MinValue = 0;
			beaconProximity.MaxValue = 3;

			switch (proximity.Value) {
			case CLProximity.Unknown:
				beaconProximity.SetValue (0, true);
				break;

			case CLProximity.Far:
				beaconProximity.SetValue (1, true);
				break;

			case CLProximity.Near:
				beaconProximity.SetValue (2, true);
				break;

			case CLProximity.Immediate:
				beaconProximity.SetValue (3, true);
				break;
			}

			return cell;
		}

		private async void GetEstimote (string id)
		{
			try {
				var location = await API.GetEstimote (id);

				Beacons [id] = location.Department;

				ReloadNavigationTitle ();

				var index = Beacons.Keys.ToList ().IndexOf (id);
				TableView.ReloadRows (new [] { NSIndexPath.FromRowSection (index, 0) }, UITableViewRowAnimation.Automatic);
			} catch (Exception) {
				// Silently fail for now
			}
		}

		private void ReloadNavigationTitle ()
		{
			NavigationItem.Title = SelectedBeacon == null ? "Geen beacon binnen bereik" : Beacons [SelectedBeacon];
		}

		private void HandleRegionEntered (object sender, CLRegionEventArgs e)
		{
			if (e.Region.Identifier == RegionId) {
				var notification = new UILocalNotification { AlertBody = "Welkom bij Freshheads!" };
				UIApplication.SharedApplication.PresentLocationNotificationNow (notification);
			}
		}

		private void HandleDidRangeBeacons (object sender, CLRegionBeaconsRangedEventArgs e)
		{
			foreach (var beacon in e.Beacons) {
				var id = beacon.Minor.ToString ();

				if (!Beacons.ContainsKey (id)) {
					Beacons.Add (id, String.Format ("Beacon {0}", beacon.Minor));
					Proximities.Add (id, beacon.Proximity);

					GetEstimote (id);
				} else {
					Proximities [id] = beacon.Proximity;
				}

				if (currentBeaconMinor != null && beacon.Minor == previousBeaconMinor && (beacon.Proximity == CLProximity.Far || previousBeaconProximity == CLProximity.Unknown)) {
					// We're monitoring a new beacon, and the previous beacon is now out of range
					SelectedBeacon = currentBeaconMinor.ToString ();

					previousBeaconMinor = currentBeaconMinor;
					previousBeaconProximity = currentBeaconProximity;
					currentBeaconMinor = null;
					currentBeaconProximity = CLProximity.Unknown;
				} else if (beacon.Minor != previousBeaconMinor && (beacon.Proximity == CLProximity.Near || beacon.Proximity == CLProximity.Immediate)) {
					// We're seeing a new beacon within close proximity
					if (previousBeaconProximity == CLProximity.Far || previousBeaconProximity == CLProximity.Unknown) {
						// The previous beacon is now out of reach, the current beacon will become the new previous beacon
						SelectedBeacon = beacon.Minor.ToString ();

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

				TableView.ReloadSections (NSIndexSet.FromIndex (0), UITableViewRowAnimation.Automatic);
			}
		}

		private void HandleDidStartMonitoringForRegion (object sender, CLRegionEventArgs e)
		{
			var indicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray);
			indicator.StartAnimating ();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (indicator);
		}

		private void HandleMonitoringFailed (object sender, EventArgs e)
		{
			NavigationItem.RightBarButtonItem = null;
		}
	}
}
