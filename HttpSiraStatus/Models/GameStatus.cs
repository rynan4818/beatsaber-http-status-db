using HttpSiraStatus.Interfaces;
using UnityEngine;

namespace HttpSiraStatus
{
    internal class GameStatus : IGameStatus
    {
#pragma warning disable IDE1006 // 命名スタイル
        public string updateCause { get; internal set; }
        public string scene { get; internal set; } = "Menu";
        public bool partyMode { get; internal set; } = false;
        public bool multiplayer { get; internal set; } = false;
        public GameModeHeadder mode { get; internal set; } = GameModeHeadder.Unknown;

        // Beatmap
        public string songName { get; internal set; } = null;
        public string songSubName { get; internal set; } = null;
        public string songAuthorName { get; internal set; } = null;
        public string levelAuthorName { get; internal set; } = null;
        public string songCover { get; internal set; } = null;
        public string songHash { get; internal set; } = null;
        public string levelId { get; internal set; } = null;
        public float songBPM { get; internal set; }
        public float noteJumpSpeed { get; internal set; }
        public float noteJumpStartBeatOffset { get; internal set; }
        public long songTimeOffset { get; internal set; } = 0;
        public long length { get; internal set; } = 0;
        public long start { get; internal set; } = 0;
        public long paused { get; internal set; } = 0;
        public string difficulty { get; internal set; } = null;
        public string difficultyEnum { get; internal set; } = null;
        public string characteristic { get; internal set; } = null;
        public int notesCount { get; internal set; } = 0;
        public int bombsCount { get; internal set; } = 0;
        public int obstaclesCount { get; internal set; } = 0;
        public int maxScore { get; internal set; } = 0;
        public string maxRank { get; internal set; } = "E";
        public string environmentName { get; internal set; } = null;
        public Color? colorSaberA { get; internal set; } = null;
        public Color? colorSaberB { get; internal set; } = null;
        public Color? colorEnvironment0 { get; internal set; } = null;
        public Color? colorEnvironment1 { get; internal set; } = null;
        public Color? colorEnvironmentBoost0 { get; internal set; } = null;
        public Color? colorEnvironmentBoost1 { get; internal set; } = null;
        public Color? colorObstacle { get; internal set; } = null;

        // Performance
        public int rawScore { get; internal set; } = 0;
        public int score { get; internal set; } = 0;
        public int currentMaxScore { get; internal set; } = 0;
        public string rank { get; internal set; } = "E";
        public int passedNotes { get; internal set; } = 0;
        public int hitNotes { get; internal set; } = 0;
        public int missedNotes { get; internal set; } = 0;
        public int lastNoteScore { get; internal set; } = 0;
        public int passedBombs { get; internal set; } = 0;
        public int hitBombs { get; internal set; } = 0;
        public int combo { get; internal set; } = 0;
        public int maxCombo { get; internal set; } = 0;
        public int multiplier { get; internal set; } = 0;
        public float multiplierProgress { get; internal set; } = 0;
        public int batteryEnergy { get; internal set; } = 1;
        public float energy { get; internal set; } = 0;
        public bool softFailed { get; internal set; } = false;
        public float relativeScore { get; internal set; } = 1;
        public int currentSongTime { get; internal set; } = 0;

        // Note cut
        public int noteID { get; internal set; } = -1;
        public string noteType { get; internal set; } = null;
        public string noteCutDirection { get; internal set; } = null;
        public string sliderHeadCutDirection { get; internal set; } = null;
        public string sliderTailCutDirection { get; internal set; } = null;
        public int noteLine { get; internal set; } = 0;
        public int noteLayer { get; internal set; } = 0;
        public int sliderHeadLine { get; internal set; } = 0;
        public int sliderHeadLayer { get; internal set; } = 0;
        public int sliderTailLine { get; internal set; } = 0;
        public int sliderTailLayer { get; internal set; } = 0;
        public bool speedOK { get; internal set; } = false;
        public bool directionOK { get; internal set; } = false;
        public bool saberTypeOK { get; internal set; } = false;
        public bool wasCutTooSoon { get; internal set; } = false;
        public int initialScore { get; internal set; } = -1;
        public int finalScore { get; internal set; } = -1;
        public int cutDistanceScore { get; internal set; } = -1;
        public int cutMultiplier { get; internal set; } = 0;
        public float saberSpeed { get; internal set; } = 0;
        public float saberDirX { get; internal set; } = 0;
        public float saberDirY { get; internal set; } = 0;
        public float saberDirZ { get; internal set; } = 0;
        public string saberType { get; internal set; } = null;
        public float swingRating { get; internal set; } = 0;
        public float beforSwingRating { get; internal set; } = 0;
        public float afterSwingRating { get; internal set; } = 0;
        public float timeDeviation { get; internal set; } = 0;
        public float cutDirectionDeviation { get; internal set; } = 0;
        public float cutPointX { get; internal set; } = 0;
        public float cutPointY { get; internal set; } = 0;
        public float cutPointZ { get; internal set; } = 0;
        public float cutNormalX { get; internal set; } = 0;
        public float cutNormalY { get; internal set; } = 0;
        public float cutNormalZ { get; internal set; } = 0;
        public float cutDistanceToCenter { get; internal set; } = 0;
        public float timeToNextBasicNote { get; internal set; } = 0;
        public string gameplayType { get; internal set; } = "";

        // Mods
        public float modifierMultiplier { get; internal set; } = 1f;
        public string modObstacles { get; internal set; } = "All";
        public bool modInstaFail { get; internal set; } = false;
        public bool modNoFail { get; internal set; } = false;
        public bool modBatteryEnergy { get; internal set; } = false;
        public int batteryLives { get; internal set; } = 1;
        public bool modDisappearingArrows { get; internal set; } = false;
        public bool modNoBombs { get; internal set; } = false;
        public string modSongSpeed { get; internal set; } = "Normal";
        public float songSpeedMultiplier { get; internal set; } = 1f;
        public bool modNoArrows { get; internal set; } = false;
        public bool modGhostNotes { get; internal set; } = false;
        public bool modFailOnSaberClash { get; internal set; } = false;
        public bool modStrictAngles { get; internal set; } = false;
        public bool modFastNotes { get; internal set; } = false;
        public bool modSmallNotes { get; internal set; } = false;
        public bool modProMode { get; internal set; } = false;
        public bool modZenMode { get; internal set; } = false;

        // Player settings
        public bool staticLights { get; internal set; } = false;
        public bool leftHanded { get; internal set; } = false;
        public float playerHeight { get; internal set; } = 1.7f;
        public float sfxVolume { get; internal set; } = 0.7f;
        public bool reduceDebris { get; internal set; } = false;
        public bool noHUD { get; internal set; } = false;
        public bool advancedHUD { get; internal set; } = false;
        public bool autoRestart { get; internal set; } = false;
        public float saberTrailIntensity { get; internal set; } = 0.5f;
        public string environmentEffects { get; internal set; } = EnvironmentEffectsFilterPreset.AllEffects.ToString();
        public bool hideNoteSpawningEffect { get; internal set; } = false;
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
