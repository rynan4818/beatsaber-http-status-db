using HttpSiraStatus.Util;
using System.Collections.Concurrent;

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
