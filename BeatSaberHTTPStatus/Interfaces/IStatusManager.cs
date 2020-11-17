using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberHTTPStatus.Interfaces
{
    public interface IStatusManager
    {
		GameStatus gameStatus { get; }
		JSONObject statusJSON { get; }
		JSONObject noteCutJSON { get; }
		JSONObject beatmapEventJSON { get; }
		event Action<StatusManager, ChangedProperties, string> statusChange;
	}
}
