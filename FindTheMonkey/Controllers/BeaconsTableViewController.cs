using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FindTheMonkey
{
	public partial class BeaconsTableViewController : UITableViewController
	{
		public BeaconsTableViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			BeaconManager.BeaconUpdated += HandleBeaconUpdated;
			BeaconManager.BeaconsUpdated += HandleBeaconsUpdated;
			BeaconManager.MonitoringStarted += HandleMonitoringStarted;
			BeaconManager.MonitoringFailed += HandleMonitoringFailed;
			BeaconManager.Initialize ();
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return BeaconManager.Beacons.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("BeaconCell");

			var beaconLabel = cell.ViewWithTag (1) as UILabel;
			var beaconProximity = cell.ViewWithTag (2) as UISlider;

			var beacon = BeaconManager.Beacons.ElementAt (indexPath.Row);
			var proximity = BeaconManager.Proximities.ElementAt (indexPath.Row);

			beaconLabel.Text = beacon.Value.ToString ();

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

		private void ReloadNavigationTitle ()
		{
			NavigationItem.Title = BeaconManager.CurrentLocation == null ? "Geen beacon binnen bereik" : BeaconManager.CurrentLocation.ToString ();
		}

		private void HandleMonitoringFailed (object sender, EventArgs e)
		{
			NavigationItem.RightBarButtonItem = null;
		}

		private void HandleMonitoringStarted (object sender, EventArgs e)
		{
			var indicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray);
			indicator.StartAnimating ();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (indicator);
		}

		private void HandleBeaconsUpdated (object sender, EventArgs e)
		{
			TableView.ReloadSections (NSIndexSet.FromIndex (0), UITableViewRowAnimation.Automatic);
		}

		private void HandleBeaconUpdated (object sender, int index)
		{
			TableView.ReloadRows (new [] { NSIndexPath.FromRowSection (index, 0) }, UITableViewRowAnimation.Automatic);

			ReloadNavigationTitle ();
		}
	}
}
