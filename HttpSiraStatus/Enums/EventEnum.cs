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
        [Description("menu")]
        Menu,
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
        AllButNoteCut = Game | Beatmap | Performance | Mod,
        PerformanceAndNoteCut = Performance | NoteCut,
        BeatmapAndPerformanceAndMod = Beatmap | Performance | Mod
    }
}
