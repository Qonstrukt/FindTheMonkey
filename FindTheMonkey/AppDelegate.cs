using System;
using Foundation;
using UIKit;

namespace FindTheMonkey
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }


		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			UIWindow.Appearance.BackgroundColor = UIColor.White;
			UITabBarItem.Appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = UIColor.White
			}, UIControlState.Normal);

			return true;
		}

		public override void ReceivedLocalNotification (UIApplication application, UILocalNotification notification)
		{
			new UIAlertView ("Freshheads", notification.AlertBody, null, "OK", null).Show ();
		}
	}
}

