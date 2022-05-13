using System;
using UnityEngine;

namespace HttpSiraStatus
{
    [Serializable]
    public class GameStatus
    {
#pragma warning disable IDE1006 // 命名スタイル
        public string updateCause { get; set; }
        public string scene { get; set; } = "Menu";
        public bool partyMode { get; set; } = false;
        public bool multiplayer { get; set; } = false;
        public GameModeHeadder mode { get; set; } = GameModeHeadder.Unknown;

        // Beatmap
        public string songName { get; set; } = null;
        public string songSubName { get; set; } = null;
        public string songAuthorName { get; set; } = null;
        public string levelAuthorName { get; set; } = null;
        public string songCover { get; set; } = null;
        public string songHash { get; set; } = null;
        public string levelId { get; set; } = null;
        public float songBPM { get; set; }
        public float noteJumpSpeed { get; set; }
        public float noteJumpStartBeatOffset { get; set; }
        public long songTimeOffset { get; set; } = 0;
        public long length { get; set; } = 0;
        public long start { get; set; } = 0;
        public long paused { get; set; } = 0;
        public string difficulty { get; set; } = null;
        public string difficultyEnum { get; set; } = null;
        public string characteristic { get; set; } = null;
        public int notesCount { get; set; } = 0;
        public int bombsCount { get; set; } = 0;
        public int obstaclesCount { get; set; } = 0;
        public int maxScore { get; set; } = 0;
        public string maxRank { get; set; } = "E";
        public string environmentName { get; set; } = null;
        public Color? colorSaberA { get; set; } = null;
        public Color? colorSaberB { get; set; } = null;
        public Color? colorEnvironment0 { get; set; } = null;
        public Color? colorEnvironment1 { get; set; } = null;
        public Color? colorEnvironmentBoost0 { get; set; } = null;
        public Color? colorEnvironmentBoost1 { get; set; } = null;
        public Color? colorObstacle { get; set; } = null;

        // Performance
        public int rawScore { get; set; } = 0;
        public int score { get; set; } = 0;
        public int currentMaxScore { get; set; } = 0;
        public string rank { get; set; } = "E";
        public int passedNotes { get; set; } = 0;
        public int hitNotes { get; set; } = 0;
        public int missedNotes { get; set; } = 0;
        public int lastNoteScore { get; set; } = 0;
        public int passedBombs { get; set; } = 0;
        public int hitBombs { get; set; } = 0;
        public int combo { get; set; } = 0;
        public int maxCombo { get; set; } = 0;
        public int multiplier { get; set; } = 0;
        public float multiplierProgress { get; set; } = 0;
        public int batteryEnergy { get; set; } = 1;
        public float energy { get; set; } = 0;
        public bool softFailed { get; set; } = false;
        public float relativeScore { get; set; } = 1;
        public int currentSongTime { get; set; } = 0;

        // Note cut
        public int noteID { get; set; } = -1;
        public string noteType { get; set; } = null;
        public string noteCutDirection { get; set; } = null;
        public string sliderHeadCutDirection { get; set; } = null;
        public string sliderTailCutDirection { get; set; } = null;
        public int noteLine { get; set; } = 0;
        public int noteLayer { get; set; } = 0;
        public int sliderHeadLine { get; set; } = 0;
        public int sliderHeadLayer { get; set; } = 0;
        public int sliderTailLine { get; set; } = 0;
        public int sliderTailLayer { get; set; } = 0;
        public bool speedOK { get; set; } = false;
        public bool directionOK { get; set; } = false;
        public bool saberTypeOK { get; set; } = false;
        public bool wasCutTooSoon { get; set; } = false;
        public int initialScore { get; set; } = -1;
        public int finalScore { get; set; } = -1;
        public int cutDistanceScore { get; set; } = -1;
        public int cutMultiplier { get; set; } = 0;
        public float saberSpeed { get; set; } = 0;
        public float saberDirX { get; set; } = 0;
        public float saberDirY { get; set; } = 0;
        public float saberDirZ { get; set; } = 0;
        public string saberType { get; set; } = null;
        public float swingRating { get; set; } = 0;
        public float beforSwingRating { get; set; } = 0;
        public float afterSwingRating { get; set; } = 0;
        public float timeDeviation { get; set; } = 0;
        public float cutDirectionDeviation { get; set; } = 0;
        public float cutPointX { get; set; } = 0;
        public float cutPointY { get; set; } = 0;
        public float cutPointZ { get; set; } = 0;
        public float cutNormalX { get; set; } = 0;
        public float cutNormalY { get; set; } = 0;
        public float cutNormalZ { get; set; } = 0;
        public float cutDistanceToCenter { get; set; } = 0;
        public float timeToNextBasicNote { get; set; } = 0;
        public string gameplayType { get; set; } = "";

        // Mods
        public float modifierMultiplier { get; set; } = 1f;
        public string modObstacles { get; set; } = "All";
        public bool modInstaFail { get; set; } = false;
        public bool modNoFail { get; set; } = false;
        public bool modBatteryEnergy { get; set; } = false;
        public int batteryLives { get; set; } = 1;
        public bool modDisappearingArrows { get; set; } = false;
        public bool modNoBombs { get; set; } = false;
        public string modSongSpeed { get; set; } = "Normal";
        public float songSpeedMultiplier { get; set; } = 1f;
        public bool modNoArrows { get; set; } = false;
        public bool modGhostNotes { get; set; } = false;
        public bool modFailOnSaberClash { get; set; } = false;
        public bool modStrictAngles { get; set; } = false;
        public bool modFastNotes { get; set; } = false;
        public bool modSmallNotes { get; set; } = false;
        public bool modProMode { get; set; } = false;
        public bool modZenMode { get; set; } = false;

        // Player settings
        public bool staticLights { get; set; } = false;
        public bool leftHanded { get; set; } = false;
        public float playerHeight { get; set; } = 1.7f;
        public float sfxVolume { get; set; } = 0.7f;
        public bool reduceDebris { get; set; } = false;
        public bool noHUD { get; set; } = false;
        public bool advancedHUD { get; set; } = false;
        public bool autoRestart { get; set; } = false;
        public float saberTrailIntensity { get; set; } = 0.5f;
        public string environmentEffects { get; set; } = EnvironmentEffectsFilterPreset.AllEffects.ToString();
        public bool hideNoteSpawningEffect { get; set; } = false;

        // Beatmap event
        public int beatmapEventType { get; set; } = 0;
        public int beatmapEventValue { get; set; } = 0;
#pragma warning restore IDE1006 // 命名スタイル
        public void ResetMapInfo()
        {
            this.songName = null;
            this.songSubName = null;
            this.songAuthorName = null;
            this.levelAuthorName = null;
            this.songCover = null;
            this.songHash = null;
            this.levelId = null;
            this.songBPM = 0f;
            this.noteJumpSpeed = 0f;
            this.songTimeOffset = 0;
            this.noteJumpStartBeatOffset = 0;
            this.length = 0;
            this.start = 0;
            this.paused = 0;
            this.difficulty = null;
            this.difficultyEnum = null;
            this.characteristic = null;
            this.notesCount = 0;
            this.obstaclesCount = 0;
            this.maxScore = 0;
            this.maxRank = "E";
            this.environmentName = null;
            this.colorSaberA = null;
            this.colorSaberB = null;
            this.colorEnvironment0 = null;
            this.colorEnvironment1 = null;
            this.colorEnvironmentBoost0 = null;
            this.colorEnvironmentBoost1 = null;
            this.colorObstacle = null;
        }

        public void ResetPerformance()
        {
            this.rawScore = 0;
            this.score = 0;
            this.currentMaxScore = 0;
            this.rank = "E";
            this.passedNotes = 0;
            this.hitNotes = 0;
            this.missedNotes = 0;
            this.lastNoteScore = 0;
            this.passedBombs = 0;
            this.hitBombs = 0;
            this.combo = 0;
            this.maxCombo = 0;
            this.multiplier = 0;
            this.multiplierProgress = 0;
            this.batteryEnergy = 1;
            this.energy = 0;
            this.softFailed = false;
            this.relativeScore = 1;
            this.currentSongTime = 0;
        }

        public void ResetNoteCut()
        {
            this.noteID = -1;
            this.noteType = null;
            this.noteCutDirection = null;
            this.sliderHeadCutDirection = null;
            this.sliderTailCutDirection = null;
            this.noteLine = 0;
            this.noteLayer = 0;
            this.sliderHeadLine = 0;
            this.sliderHeadLayer = 0;
            this.sliderTailLine = 0;
            this.sliderTailLayer = 0;
            this.speedOK = false;
            this.directionOK = false;
            this.saberTypeOK = false;
            this.wasCutTooSoon = false;
            this.initialScore = -1;
            this.finalScore = -1;
            this.cutDistanceScore = -1;
            this.cutMultiplier = 0;
            this.saberSpeed = 0;
            this.saberDirX = 0;
            this.saberDirY = 0;
            this.saberDirZ = 0;
            this.saberType = null;
            this.swingRating = 0;
            this.beforSwingRating = 0;
            this.afterSwingRating = 0;
            this.timeDeviation = 0;
            this.cutDirectionDeviation = 0;
            this.cutPointX = 0;
            this.cutPointY = 0;
            this.cutPointZ = 0;
            this.cutNormalX = 0;
            this.cutNormalY = 0;
            this.cutNormalZ = 0;
            this.cutDistanceToCenter = 0;
            this.gameplayType = "";
        }
    }
}
