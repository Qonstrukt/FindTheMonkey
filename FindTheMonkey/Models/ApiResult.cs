using System;
using Newtonsoft.Json;

namespace Freshheads.SmartRoom.iOS
{
	[JsonObject]
	public class ApiResult
	{
		[JsonProperty ("message")]
		public string Message { get; set; }

		[JsonProperty ("user")]
		public User User { get; set; }

		[JsonProperty ("users")]
		public int[] Users { get; set; }


		public bool FirstUserInRoom {
			get { return Users.Length == 1; }
		}

		public bool UsersLeftInRoom {
			get { return Users.Length > 0; }
		}
	}
}

