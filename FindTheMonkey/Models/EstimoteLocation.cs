using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Freshheads.Library;

namespace FindTheMonkey
{
	[JsonObject (MemberSerialization.OptIn)]
	public class EstimoteLocation : BaseModel
	{
		[JsonProperty ("department")]
		public string Department { get; set; }

		[JsonProperty ("members")]
		public List<DepartmentMember> Members { get; set; }


		public override string ToString ()
		{
			return Department;
		}
	}
}

