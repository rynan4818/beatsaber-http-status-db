using HttpSiraStatus.Installer;
using IPA;
using SiraUtil.Zenject;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

// Interesting props and methods:
// protected const int ScoreController.kMaxCutScore // 110
// public BeatmapObjectSpawnController.noteWasCutEvent<BeatmapObjectSpawnController, NoteController, NoteCutInfo> // Listened to by scoreManager for its cut event and therefore is raised before combo, multiplier and score changes
// public BeatmapObjectSpawnController.noteWasMissedEvent<BeatmapObjectSpawnController, NoteController> // Same as above, but for misses
// public BeatmapObjectSpawnController.obstacleDidPassAvoidedMarkEvent<BeatmapObjectSpawnController, ObstacleController>
// public int ScoreController.prevFrameScore
// protected ScoreController._baseScore

namespace HttpSiraStatus
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        /// <summary>
        /// Populated by MSBuild
        /// </summary>
        public static string PluginVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string GameVersion => Application.version;

        public string Name => Assembly.GetExecutingAssembly().GetName().Name;
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
            Logger.Debug($"Game version : {GameVersion}");
        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }
    }
}
