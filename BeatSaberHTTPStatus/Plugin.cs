using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using BS_Utils.Gameplay;
using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using BeatSaberHTTPStatus.Installer;

// Interesting props and methods:
// protected const int ScoreController.kMaxCutScore // 110
// public BeatmapObjectSpawnController.noteWasCutEvent<BeatmapObjectSpawnController, NoteController, NoteCutInfo> // Listened to by scoreManager for its cut event and therefore is raised before combo, multiplier and score changes
// public BeatmapObjectSpawnController.noteWasMissedEvent<BeatmapObjectSpawnController, NoteController> // Same as above, but for misses
// public BeatmapObjectSpawnController.obstacleDidPassAvoidedMarkEvent<BeatmapObjectSpawnController, ObstacleController>
// public int ScoreController.prevFrameScore
// protected ScoreController._baseScore

namespace BeatSaberHTTPStatus {
	[Plugin(RuntimeOptions.SingleStartInit)]
	internal class Plugin
	{
		public static readonly string PluginVersion = "$SEMVER_VERSION$"; // Populated by MSBuild
		public static readonly string GameVersion = "$BS_VERSION$"; // Populated by MSBuild

		public string Name {
			get {return "HTTP Status";}
		}

		public string Version {
			get {return PluginVersion;}
		}

		public static IPALogger Logger { get; private set; }

		[Init]
		public void Init(IPALogger logger, Zenjector zenjector) {
			Logger = logger;
			Logger.Debug("Logger Initialized.");
			zenjector.OnGame<HttpGameInstaller>();
			zenjector.OnApp<HttpAppInstaller>();
		}

		[OnStart]
		public void OnApplicationStart()
		{

		}

		[OnExit]
		public void OnApplicationQuit() {
			
		}
	}
}
