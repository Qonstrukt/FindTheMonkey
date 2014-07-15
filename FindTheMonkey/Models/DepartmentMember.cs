using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Freshheads.Library;

namespace FindTheMonkey
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DepartmentMember : BaseModel
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("gender")]
		public string Gender { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("image")]
		public string Image { get; set; }
	}
}

