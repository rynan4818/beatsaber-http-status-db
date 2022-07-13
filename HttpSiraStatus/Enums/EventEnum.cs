using System;
using System.ComponentModel;

namespace HttpSiraStatus
{
    /// <summary>
    /// 拡張メソッド用クラス
    /// </summary>
    public static class EnumExtention
    {
        /// <summary>
        /// <see cref="Enum"/>の<see cref="DescriptionAttribute"/>属性に指定された文字列を取得する拡張メソッドです。
        /// </summary>
        /// <param name="value">文字列を取得したい<see cref="Enum"/></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
                return attribute.Description;
            }
            else {
                return value.ToString();
            }
        }
    }

    /// <summary>
    /// イベントの名称と値を管理します。
    /// </summary>
    public enum BeatSaberEvent
    {
        [Description("other")]
        Other = int.MaxValue,
        [Description("menu")]
        Menu = 0,
        [Description("songStart")]
        SongStart,
        [Description("obstacleEnter")]
        ObstacleEnter,
        [Description("obstacleExit")]
        ObstacleExit,
        [Description("pause")]
        Pause,
        [Description("resume")]
        Resume,
        [Description("bombCut")]
        BombCut,
        [Description("noteSpawned")]
        NoteSpawned,
        [Description("noteCut")]
        NoteCut,
        [Description("noteFullyCut")]
        NoteFullyCut,
        [Description("bombMissed")]
        BombMissed,
        [Description("noteMissed")]
        NoteMissed,
        [Description("scoreChanged")]
        ScoreChanged,
        [Description("finished")]
        Finished,
        [Description("failed")]
        Failed,
        [Description("beatmapEvent")]
        BeatmapEvent,
        [Description("energyChanged")]
        EnergyChanged,
        [Description("softFailed")]
        SoftFailed
    }

    /// <summary>
    /// 変更されたプロパティを管理する列挙型
    /// クラスじゃないのでパフォーマンス面でHTTPStatusより良いはず。
    /// </summary>
    [Flags]
    public enum ChangedProperty
    {
        Game = 1,
        Beatmap = 1 << 1,
        Performance = 1 << 2,
        NoteCut = 1 << 3,
        Mod = 1 << 4,
        BeatmapEvent = 1 << 5,
        Other = 1 << 31,
        AllButNoteCut = Game | Beatmap | Performance | Mod,
        PerformanceAndNoteCut = Performance | NoteCut,
        BeatmapAndPerformanceAndMod = Beatmap | Performance | Mod
    }

    public enum GameModeHeadder
    {
        Unknown,
        [Description("Solo")]
        Solo,
        [Description("Party")]
        Party,
        [Description("Multiplayer")]
        Multiplayer,
    }


    /// <summary>
    /// BeatmapEventType
    /// </summary>
    /// <remarks>https://github.com/Caeden117/ChroMapper/blob/master/Assets/__Scripts/Map/Events/MapEvent.cs</remarks>
    public enum BeatmapEventType
    {
        BackLasers = 0,
        RingLights = 1,
        LeftLasers = 2,
        RightLasers = 3,
        RoadLights = 4,
        BoostLights = 5,
        CustomLight2 = 6,
        CustomLight3 = 7,
        RingsRotate = 8,
        RingsZoom = 9,
        CustomLight4 = 10,
        CustomLight5 = 11,
        LeftLasersSpeed = 12,
        RightLasersSpeed = 13,
        EarlyRotation = 14,
        LateRotation = 15,
        CustomEvent1 = 16,
        CustomEvent2 = 17,
    }

    /// <summary>
    /// BeatmapEventLightValue
    /// </summary>
    public enum BeatmapEventLightValue
    {
        Off = 0,

        BlueON = 1,
        BlueFlash = 2,
        BlueFade = 3,

        RedON = 5,
        RedFlash = 6,
        RedFade = 7,
    }
}
