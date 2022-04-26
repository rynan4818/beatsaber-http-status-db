#pragma warning disable IDE1006 // 命名スタイル
namespace HttpSiraStatus.Interfaces
{
    public interface IBeatmapObjectEntity
    {
        ColorType colorType { get; }
        float time { get; }
        int executionOrder { get; }
        int subtypeIdentifier { get; }
        BeatmapDataItem.BeatmapDataItemType type { get; }
    }
}
