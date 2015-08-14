using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Foundation;
using Refit;
using UIKit;

namespace Freshheads.SmartRoom.iOS
{
	public partial class BeaconsTableViewController : UITableViewController, IBeaconManagerDelegate
	{
		protected ISmartRoomApi Api;
		protected List<CLBeacon> Beacons;


		public BeaconsTableViewController (IntPtr handle) : base (handle)
		{
			Api = RestService.For<ISmartRoomApi> ("http://sander.dev.freshheads.local/smart-room/web/app_debug.php");
			Beacons = new List<CLBeacon> ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			BeaconManager.Delegate = this;
			BeaconManager.RegisterBeaconRegion ("74278bda-b644-4520-8f0c-720eaf059935", 1, "Kamer 1");
			BeaconManager.RegisterBeaconRegion ("74278bda-b644-4520-8f0c-720eaf059935", 2, "Kamer 2");
			BeaconManager.RegisterBeaconRegion ("74278bda-b644-4520-8f0c-720eaf059935", 3, "Kamer 3");
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return Beacons.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("BeaconCell", indexPath);

			var beaconLabel = cell.ViewWithTag (1) as UILabel;
			var beaconProximity = cell.ViewWithTag (2) as UIProgressView;

			var beacon = Beacons [indexPath.Row];

			beaconLabel.Enabled = true;
			beaconLabel.Text = String.Format ("Beacon {0}:{1}", beacon.Major, beacon.Minor);

			beaconProximity.Hidden = false;

			switch (beacon.Proximity) {
			case CLProximity.Unknown:
				beaconLabel.Enabled = false;
				beaconProximity.Hidden = true;
				beaconProximity.Progress = 0;
				break;

			case CLProximity.Far:
				beaconProximity.Progress = 0;
				break;

			case CLProximity.Near:
				beaconProximity.Progress = 0.5f;
				break;

			case CLProximity.Immediate:
				beaconProximity.Progress = 1;
				break;
			}

			return cell;
		}


		#region IBeaconManagerDelegate implementation

		public void BeaconsRanged (CLBeacon[] beacons, CLBeaconRegion region)
		{
			foreach (var beacon in beacons) {
				Beacons.RemoveAll (obj => beacon.Major == obj.Major && beacon.Minor == obj.Minor);
				Beacons.Add (beacon);
			}

			Beacons = Beacons.OrderBy (BeaconManager.OrderByProximity).ToList ();

			TableView.ReloadData ();
		}

		public void MonitoringStarted (CLBeaconRegion region)
		{
			var indicator = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White);
			indicator.StartAnimating ();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (indicator);
		}

		public void MonitoringFailed ()
		{
			NavigationItem.RightBarButtonItem = null;
		}

		public async void RegionEnetered (CLBeaconRegion region, CLBeaconRegion previous)
		{
			if (previous == null) {
				var result = await Api.UserEnterRoom (AppDelegate.UserId, (int)region.Major);

				AppDelegate.PreferedColor = result.User.PreferedColor;

				if (result.FirstUserInRoom)
					ShowTurnLightsOnNotification (region);
			
				return;
			} 

			if (region.Identifier != previous.Identifier) {
				var leftResult = await Api.UserLeftRoom (AppDelegate.UserId, (int)region.Major);
				var enterResult = await Api.UserEnterRoom (AppDelegate.UserId, (int)region.Major);

				if (!leftResult.UsersLeftInRoom && enterResult.FirstUserInRoom) {
					ShowSwitchLightsNotification (region, previous);
					return;
				}

				if (!leftResult.UsersLeftInRoom) {
					ShowTurnLightsOffNotification (region);
					return;
				}

				if (enterResult.FirstUserInRoom) {
					ShowTurnLightsOnNotification (region);
					return;
				}
			}
		}

		public async void RegionLeft (CLBeaconRegion region)
		{
			var result = await Api.UserLeftRoom (AppDelegate.UserId, (int)region.Major);
			if (!result.UsersLeftInRoom)
				ShowTurnLightsOffNotification (region);
		}

		#endregion


		private void ShowTurnLightsOnNotification (CLBeaconRegion region)
		{
			var notification = new UILocalNotification { 
				AlertBody = String.Format ("Wil je de lichten aanzetten in {0}?", region.Identifier),
				Category = AppDelegate.EnterRoomCategory,
				SoundName = UILocalNotification.DefaultSoundName,
				UserInfo = new NSDictionary ("region", region.Major)
			};
			BeginInvokeOnMainThread (() => UIApplication.SharedApplication.PresentLocalNotificationNow (notification));
		}

		private void ShowTurnLightsOffNotification (CLBeaconRegion region)
		{
			var notification = new UILocalNotification { 
				AlertBody = String.Format ("Is alles opgeruimd, en wil je de lichten uitzetten in {0}?", region.Identifier),
				Category = AppDelegate.LeftRoomCategory,
				SoundName = UILocalNotification.DefaultSoundName,
				UserInfo = new NSDictionary ("region", region.Major)
			};
			BeginInvokeOnMainThread (() => UIApplication.SharedApplication.PresentLocalNotificationNow (notification));
		}

		private void ShowSwitchLightsNotification (CLBeaconRegion region, CLBeaconRegion previous)
		{
			var notification = new UILocalNotification { 
				AlertBody = String.Format ("Wil je de lichten aanzetten in {0} en uitzetten in {1}?", region.Identifier, previous.Identifier),
				Category = AppDelegate.SwitchRoomCategory,
				SoundName = UILocalNotification.DefaultSoundName,
				UserInfo = new NSDictionary ("region", region.Major, "previous", previous.Major)
			};
			BeginInvokeOnMainThread (() => UIApplication.SharedApplication.PresentLocalNotificationNow (notification));			
		}
	}
}
