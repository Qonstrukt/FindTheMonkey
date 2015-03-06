using System;
using System.Linq;
using CoreLocation;
using Foundation;
using UIKit;

namespace FindTheMonkey
{
	partial class DepartmentDetailPageViewController : UIPageViewController, IBeaconManagerDelegate
	{
		private BeaconRestClient restClient;
		private CLBeacon currentBeacon;


		public DepartmentDetailPageViewController (IntPtr handle) : base (handle)
		{
		}

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			restClient = BeaconRestClient.SharedInstance;

			var searchingViewController = Storyboard.InstantiateViewController ("SearchingDepartment");

			await SetViewControllersAsync (new [] { searchingViewController }, UIPageViewControllerNavigationDirection.Forward, false);

			BeaconManager.Delegate = this;
		}

		#region IBeaconManagerDelegate implementation

		public async void BeaconsRanged (CLBeacon[] beacons, CLBeaconRegion region)
		{
			if (BeaconManager.RangedBeacons.Count == 0)
				return;

			var firstBeacon = BeaconManager.RangedBeacons.First ().Value;

			if ((currentBeacon == null || firstBeacon.Minor != currentBeacon.Minor) && firstBeacon.Proximity < CLProximity.Far) {
				try {
					currentBeacon = firstBeacon;
					var location = await restClient.GetEstimote (firstBeacon.Minor.ToString ());

					var detailViewController = Storyboard.InstantiateViewController ("DepartmentDetail") as DepartmentDetailTableViewController;
					detailViewController.Location = location;

					NavigationItem.Title = location.Department;

					await SetViewControllersAsync (new [] { detailViewController }, UIPageViewControllerNavigationDirection.Forward, true);
				} catch (Exception exception) {
//					new UIAlertView ("Er is een fout opgetreden", exception.Message, null, "OK", null).Show ();
				}
			}
		}

		public void MonitoringStarted (CLRegion region)
		{

		}

		public void MonitoringFailed ()
		{

		}

		public void RegionEnetered (CLRegion region, CLRegion previous)
		{

		}

		#endregion
	}
}
