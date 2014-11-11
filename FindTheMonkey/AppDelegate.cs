using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FindTheMonkey
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }


		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			return true;
		}

		public override void ReceivedLocalNotification (UIApplication application, UILocalNotification notification)
		{
			new UIAlertView ("Freshheads", notification.AlertBody, null, "OK", null).Show ();
		}
	}
}

