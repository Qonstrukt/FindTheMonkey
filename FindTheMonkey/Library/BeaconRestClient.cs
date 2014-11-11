using System;
using System.Net.Http;
using System.Threading.Tasks;

using Freshheads.Library.Rest;

namespace FindTheMonkey
{
	public class BeaconRestClient : RestClient
	{
		protected override string BaseURL {
			get {
				return "http://mocksvc.mulesoft.com/mocks/d072152f-cd7e-4ed5-a5b6-d871b8e98264/";
			}
		}

		static private BeaconRestClient _sharedInstance;

		static public BeaconRestClient SharedInstance {
			get {
				if (_sharedInstance == null) {
					_sharedInstance = new BeaconRestClient ();
				}
				return _sharedInstance;
			}
		}

		public Task<EstimoteLocation> GetEstimote (string id)
		{
			var request = new RestRequestMessage (HttpMethod.Get, String.Format ("estimotes/{0}", id));
			return ExecuteAsync<EstimoteLocation> (request, null);
		}
	}
}

