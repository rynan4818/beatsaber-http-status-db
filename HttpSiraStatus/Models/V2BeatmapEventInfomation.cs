using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using Zenject;

namespace HttpSiraStatus.Models
{
    internal class V2BeatmapEventInfomation : IBeatmapEventInformation
    {
        public string version { get; } = "2.6.0";
        public int beatmapEventType { get; internal set; } = 0;
        public int beatmapEventValue { get; internal set; } = 0;
        public float beatmapEventFloatValue { get; internal set; } = 0;

        public void Init(BeatmapEventData eventData)
        {
            if (eventData is BasicBeatmapEventData basic) {
                this.beatmapEventType = (int)basic.basicBeatmapEventType;
                this.beatmapEventValue = basic.value;
                this.beatmapEventFloatValue = basic.floatValue;
            }
        }

        public void Reset()
        {
            this.beatmapEventType = 0;
            this.beatmapEventValue = 0;
        }

        public JSONObject ToJson()
        {
            var result = new JSONObject();
            result["version"] = this.version;
            result["type"] = this.beatmapEventType;
            result["value"] = this.beatmapEventValue;
            result["floatValue"] = this.beatmapEventFloatValue;
            return result;
        }

        public class Pool : MemoryPool<V2BeatmapEventInfomation>
        {
            protected override void OnDespawned(V2BeatmapEventInfomation item)
            {
                item.Reset();
            }
        }
    }
}
