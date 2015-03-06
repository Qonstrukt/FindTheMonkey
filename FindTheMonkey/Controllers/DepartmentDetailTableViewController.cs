using System;
using Foundation;
using UIKit;
using Freshheads.Library.iOS;

namespace FindTheMonkey
{
	partial class DepartmentDetailTableViewController : UITableViewController
	{
		public EstimoteLocation Location;


		public DepartmentDetailTableViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (Location == null)
				throw new NullReferenceException ("Location cannot be null");
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return Location.Members.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("EmployeeCell");

			var employee = Location.Members [indexPath.Row];

			var imageView = cell.ViewWithTag (1) as UIRemoteImageView;
			imageView.Layer.CornerRadius = imageView.Bounds.Width / 2;
			imageView.SetRemoteImageURLAsync (employee.Image);

			var nameLabel = cell.ViewWithTag (2) as UILabel;
			nameLabel.Text = employee.ToString ();

			return cell;
		}
	}
}
