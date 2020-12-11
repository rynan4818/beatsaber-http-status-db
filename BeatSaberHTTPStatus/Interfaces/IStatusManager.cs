using SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberHTTPStatus.Interfaces
{
    public interface IStatusManager
    {
		GameStatus GameStatus { get; }
		JSONObject StatusJSON { get; }
		JSONObject NoteCutJSON { get; }
		JSONObject BeatmapEventJSON { get; }
		ConcurrentQueue<JSONObject> JsonQueue { get; }
		//event StatusChangedEventHandler StatusChanged;
		void EmitStatusUpdate(ChangedProperties changedProps, string cause);
	}
}
