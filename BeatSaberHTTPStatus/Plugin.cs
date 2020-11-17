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
	[Plugin(RuntimeOptions.DynamicInit)]
	internal class Plugin {
		public static Plugin instance {get; private set;}
		public static readonly string PluginVersion = "$SEMVER_VERSION$"; // Populated by MSBuild
		public static readonly string GameVersion = "$BS_VERSION$"; // Populated by MSBuild

		public string Name {
			get {return "HTTP Status";}
		}

		public string Version {
			get {return PluginVersion;}
		}

		public static IPALogger log;

		[Init]
		public void Init(IPALogger logger, Zenjector zenjector) {
			log = logger;
			zenjector.OnApp<HttpAppInstaller>();
			zenjector.OnGame<HttpGameInstaller>();
		}

		[OnStart]
		public void OnApplicationStart() {
			if (instance != null) return;
			instance = this;
		}

		[OnExit]
		public void OnApplicationQuit() {
			
		}
	}
}
