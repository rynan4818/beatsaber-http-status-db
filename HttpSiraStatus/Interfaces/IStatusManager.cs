using HttpSiraStatus.Util;
using System;
using System.Collections.Concurrent;

namespace HttpSiraStatus.Interfaces
{
    public interface IStatusManager
    {
        [Obsolete("このプロパティはいずれなくなります。DiContainerからInjectで取得してください。", true)]
        IGameStatus GameStatus { get; }
        JSONObject StatusJSON { get; }
        JSONObject NoteCutJSON { get; }
        JSONObject BeatmapEventJSON { get; }
        JSONObject OtherJSON { get; }
        ConcurrentQueue<JSONObject> JsonQueue { get; }
        event SendEventHandler SendEvent;
        void EmitStatusUpdate(ChangedProperty changedProps, BeatSaberEvent e);
    }
}
