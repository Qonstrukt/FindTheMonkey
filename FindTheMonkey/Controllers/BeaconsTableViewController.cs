using System;
using System.Collections.Generic;
using System.Linq;

using CoreLocation;
using Foundation;
using UIKit;

namespace FindTheMonkey
{
	public partial class BeaconsTableViewController : UITableViewController, IBeaconManagerDelegate
	{
		public BeaconsTableViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			BeaconManager.Delegate = this;
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return BeaconManager.RangedBeacons.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("BeaconCell");

			var beaconLabel = cell.ViewWithTag (1) as UILabel;
			var beaconProximity = cell.ViewWithTag (2) as UISlider;

			var beacon = BeaconManager.RangedBeacons.ElementAt (indexPath.Row);

			beaconLabel.Text = beacon.Value.ToString ();

			beaconProximity.MinValue = 0;
			beaconProximity.MaxValue = 3;

			switch (beacon.Value.Proximity) {
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

		private void ReloadNavigationTitle ()
		{
//			NavigationItem.Title = BeaconManager.CurrentLocation == null ? "Geen beacon binnen bereik" : BeaconManager.CurrentLocation.ToString ();
		}


		#region IBeaconManagerDelegate implementation

		public void BeaconUpdated (int beaconIndex)
		{
			TableView.ReloadRows (new [] { NSIndexPath.FromRowSection (beaconIndex, 0) }, UITableViewRowAnimation.Automatic);

			ReloadNavigationTitle ();
		}

		public void BeaconsRanged (CLBeacon[] beacons, CLBeaconRegion region)
		{
			TableView.ReloadSections (NSIndexSet.FromIndex (0), UITableViewRowAnimation.Automatic);
		}

		public void MonitoringStarted (CLRegion region)
		{
			var indicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray);
			indicator.StartAnimating ();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (indicator);
		}

		public void MonitoringFailed ()
		{
			NavigationItem.RightBarButtonItem = null;
		}

		public void RegionEnetered (CLRegion region, CLRegion previous)
		{
			if (region.Identifier != previous.Identifier) {
				var notification = new UILocalNotification { AlertBody = "Welkom bij Freshheads!" };
				UIApplication.SharedApplication.PresentLocalNotificationNow (notification);
			}
		}

		#endregion
	}
}
