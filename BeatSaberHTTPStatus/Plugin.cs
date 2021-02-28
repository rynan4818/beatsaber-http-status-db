using BeatSaberHTTPStatus.Installer;
using IPA;
using SiraUtil.Zenject;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

// Interesting props and methods:
// protected const int ScoreController.kMaxCutScore // 110
// public BeatmapObjectSpawnController.noteWasCutEvent<BeatmapObjectSpawnController, NoteController, NoteCutInfo> // Listened to by scoreManager for its cut event and therefore is raised before combo, multiplier and score changes
// public BeatmapObjectSpawnController.noteWasMissedEvent<BeatmapObjectSpawnController, NoteController> // Same as above, but for misses
// public BeatmapObjectSpawnController.obstacleDidPassAvoidedMarkEvent<BeatmapObjectSpawnController, ObstacleController>
// public int ScoreController.prevFrameScore
// protected ScoreController._baseScore

namespace BeatSaberHTTPStatus
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        public static readonly string PluginVersion = "$SEMVER_VERSION$"; // Populated by MSBuild
        public static readonly string GameVersion = "$BS_VERSION$"; // Populated by MSBuild

        public string Name
        {
            get { return "HTTP Status"; }
        }

        public string Version
        {
            get { return PluginVersion; }
        }

        public static IPALogger Logger { get; private set; }

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            Logger = logger;
            Logger.Debug("Logger Initialized.");
            zenjector.OnGame<HttpGameInstaller>(true).OnlyForStandard();
            zenjector.OnGame<HttpGameInstaller>(false).OnlyForMultiplayer();
            zenjector.OnApp<HttpAppInstaller>();
        }

        [OnStart]
        public void OnApplicationStart()
        {

        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }
    }
}
