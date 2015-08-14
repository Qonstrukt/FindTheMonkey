using System;
using Foundation;
using Refit;
using UIKit;

namespace Freshheads.SmartRoom.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public const int UserId = 1;

		public const string SwitchRoomCategory = "SwitchRoom";
		public const string EnterRoomCategory = "EnterRoom";
		public const string LeftRoomCategory = "LeftRoom";
		public const string LightsSwitchIdentifier = "LightsSwitch";
		public const string LightsOnIdentifier = "LightsOn";
		public const string LightsOffIdentifier = "LightsOff";

		public static string PreferedColor { get; set; }


		public override UIWindow Window { get; set; }

		protected ISmartRoomApi Api;


		public override void FinishedLaunching (UIApplication application)
		{
			Api = RestService.For<ISmartRoomApi> ("http://sander.dev.freshheads.local/smart-room/web/app_debug.php");

			UIWindow.Appearance.BackgroundColor = UIColor.White;
			UITabBarItem.Appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = UIColor.White
			}, UIControlState.Normal);

			RegisterNotifications (application);
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			FinishedLaunching (application);

			return true;
		}

		public override async void HandleAction (UIApplication application, string actionIdentifier, UILocalNotification localNotification, Action completionHandler)
		{
			var dictionary = localNotification.UserInfo;
			var region = dictionary.ObjectForKey (new NSString ("region")) as NSNumber;
			var previous = dictionary.ObjectForKey (new NSString ("previous")) as NSNumber;

			switch (actionIdentifier) {
			case LightsSwitchIdentifier:
				await Api.ToggleLight (previous.Int32Value, 0);
				await Api.ToggleLight (region.Int32Value, 1, PreferedColor);
				break;

			case LightsOnIdentifier:
				await Api.ToggleLight (region.Int32Value, 1, PreferedColor);
				break;

			case LightsOffIdentifier:
				await Api.ToggleLight (region.Int32Value, 0);
				break;
			}

			completionHandler.Invoke ();
		}

		public override void ReceivedLocalNotification (UIApplication application, UILocalNotification notification)
		{
			if (application.ApplicationState == UIApplicationState.Active)
				new UIAlertView ("SmartRoom", notification.AlertBody, null, "OK", null).Show ();
		}

		protected void RegisterNotifications (UIApplication application)
		{
			var lightsSwitchAction = new UIMutableUserNotificationAction {
				Identifier = LightsSwitchIdentifier,
				Title = "Zet lampen uit en aan",
				ActivationMode = UIUserNotificationActivationMode.Background,
				AuthenticationRequired = false,
				Destructive = true
			};
			var lightsOnAction = new UIMutableUserNotificationAction {
				Identifier = LightsOnIdentifier,
				Title = "Zet lampen aan",
				ActivationMode = UIUserNotificationActivationMode.Background,
				AuthenticationRequired = false
			};
			var lightsOffAction = new UIMutableUserNotificationAction {
				Identifier = LightsOffIdentifier,
				Title = "Zet lampen uit",
				ActivationMode = UIUserNotificationActivationMode.Background,
				AuthenticationRequired = false,
				Destructive = true
			};
					
			var switchRoomCategory = new UIMutableUserNotificationCategory {
				Identifier = SwitchRoomCategory
			};
			switchRoomCategory.SetActions (new UIUserNotificationAction[] {
				lightsSwitchAction, lightsOnAction, lightsOffAction
			}, UIUserNotificationActionContext.Default);
			switchRoomCategory.SetActions (new UIUserNotificationAction[] {
				lightsSwitchAction, lightsOnAction, lightsOffAction
			}, UIUserNotificationActionContext.Minimal);

			var enterRoomCategory = new UIMutableUserNotificationCategory {
				Identifier = EnterRoomCategory
			};
			enterRoomCategory.SetActions (new UIUserNotificationAction[] { lightsOnAction }, UIUserNotificationActionContext.Default);
			enterRoomCategory.SetActions (new UIUserNotificationAction[] { lightsOnAction }, UIUserNotificationActionContext.Minimal);

			var leftRoomCategory = new UIMutableUserNotificationCategory {
				Identifier = LeftRoomCategory
			};
			leftRoomCategory.SetActions (new UIUserNotificationAction[] { lightsOffAction }, UIUserNotificationActionContext.Default);
			leftRoomCategory.SetActions (new UIUserNotificationAction[] { lightsOffAction }, UIUserNotificationActionContext.Minimal);

			var categories = new NSSet (switchRoomCategory, enterRoomCategory, leftRoomCategory);
			var settings = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, categories);
			application.RegisterUserNotificationSettings (settings);
		}
	}
}

