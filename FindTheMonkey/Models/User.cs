using System;
using Newtonsoft.Json;

namespace Freshheads.SmartRoom.iOS
{
	[JsonObject]
	public class User
	{
		[JsonProperty ("id")]
		public int Id { get; set; }

		[JsonProperty ("name")]
		public string Name { get; set; }

		[JsonProperty ("prefered_color")]
		public string PreferedColor { get; set; }

		[JsonProperty ("in_room")]
		public int? InRoom { get; set; }
	}
}

