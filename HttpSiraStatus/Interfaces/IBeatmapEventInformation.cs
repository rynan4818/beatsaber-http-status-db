using HttpSiraStatus.Util;

namespace HttpSiraStatus.Interfaces
{
    public interface IBeatmapEventInformation
    {
        public string version { get; }
        void Init(BeatmapEventData eventData);
        void Reset();
        JSONObject ToJson();
    }
}
