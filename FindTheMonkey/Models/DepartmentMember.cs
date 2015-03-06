using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Freshheads.Library;

namespace FindTheMonkey
{
	[JsonObject (MemberSerialization.OptIn)]
	public class DepartmentMember : BaseModel
	{
		[JsonProperty ("name")]
		public string Name { get; set; }

		[JsonProperty ("title")]
		public string Title { get; set; }

		[JsonProperty ("gender")]
		public string Gender { get; set; }

		[JsonProperty ("email")]
		public string Email { get; set; }

		[JsonProperty ("image")]
		public string Image { get; set; }

	
		[JsonProperty ("additionalName")]
		public string AdditionalName { get; set; }

		[JsonProperty ("familyName")]
		public string FamiltyName { get; set; }

		[JsonProperty ("givenName")]
		public string GivenName { get; set; }


		public override string ToString ()
		{
			return Name ?? String.Join (" ", GivenName, AdditionalName, FamiltyName);
		}
	}
}

