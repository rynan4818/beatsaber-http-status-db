using HttpSiraStatus.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpSiraStatus.Interfaces
{
    public interface IStatusManager
    {
		GameStatus GameStatus { get; }
		JSONObject StatusJSON { get; }
		JSONObject NoteCutJSON { get; }
		JSONObject BeatmapEventJSON { get; }
		ConcurrentQueue<JSONObject> JsonQueue { get; }
		event SendEventHandler SendEvent;
		void EmitStatusUpdate(ChangedProperty changedProps, BeatSaberEvent e);
	}
}
