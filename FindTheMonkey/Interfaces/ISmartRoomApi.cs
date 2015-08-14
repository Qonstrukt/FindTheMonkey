using System;
using System.Threading.Tasks;
using Refit;

namespace Freshheads.SmartRoom.iOS
{
	public interface ISmartRoomApi
	{
		[Get ("/user/{userId}/enter-room/{roomId}")]
		Task<ApiResult> UserEnterRoom (int userId, int roomId);

		[Get ("/user/{userId}/exit-room/{roomId}")]
		Task<ApiResult> UserLeftRoom (int userId, int roomId);

		[Get ("/light/{lightId}/toggle/{switchValue}")]
		Task<ApiResult> ToggleLight (int lightId, int switchValue, string color = null);
	}
}

