using BeatSaberHTTPStatus.Interfaces;
using BeatSaberHTTPStatus.Util;
using BS_Utils.Gameplay;
using IPA.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatSaberHTTPStatus.Models
{
    public class GamePlayDataManager : IInitializable, IDisposable
    {
		[Inject]
		DiContainer container;
		[Inject]
		private IStatusManager statusManager;
		[Inject]
		GameStatus gameStatus;

		private GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
		private PauseController pauseController;
		private ScoreController scoreController;
		private GameplayModifiers gameplayModifiers;
		private AudioTimeSyncController audioTimeSyncController;
		private BeatmapObjectCallbackController beatmapObjectCallbackController;
		private PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction;
		private GameSongController gameSongController;
		private GameEnergyCounter gameEnergyCounter;
		private ConcurrentDictionary<NoteCutInfo, NoteData> noteCutMapping = new ConcurrentDictionary<NoteCutInfo, NoteData>();
		
		private GameplayModifiersModelSO gameplayModifiersSO;

		/// private PlayerHeadAndObstacleInteraction ScoreController._playerHeadAndObstacleInteraction;
		//private FieldInfo scoreControllerHeadAndObstacleInteractionField = typeof(ScoreController).GetField("_playerHeadAndObstacleInteraction", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		/// protected NoteCutInfo CutScoreBuffer._noteCutInfo
		private FieldInfo noteCutInfoField = typeof(CutScoreBuffer).GetField("_noteCutInfo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		/// protected List<CutScoreBuffer> ScoreController._cutScoreBuffers // contains a list of after cut buffers
		private FieldInfo afterCutScoreBuffersField = typeof(ScoreController).GetField("_cutScoreBuffers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		/// private int CutScoreBuffer#_multiplier
		private FieldInfo cutScoreBufferMultiplierField = typeof(CutScoreBuffer).GetField("_multiplier", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		/// private static LevelCompletionResults.Rank LevelCompletionResults.GetRankForScore(int score, int maxPossibleScore)
		//private MethodInfo getRankForScoreMethod = typeof(LevelCompletionResults).GetMethod("GetRankForScore", BindingFlags.NonPublic | BindingFlags.Static);
		private bool headInObstacle = false;

		/// <summary>
		/// Beat Saber 1.12.1 removes NoteData.id, forcing us to generate our own note IDs to allow users to easily link events about the same note.
		/// Before 1.12.1 the noteID matched the note order in the beatmap file, but this is impossible to replicate now without hooking into the level loading code.
		/// </summary>
		private NoteData[] noteToIdMapping = null;
		private int lastNoteId = 0;

		private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
					// TODO: マネージド状態を破棄します (マネージド オブジェクト)
					Plugin.log.Debug("dispose call");
                    try {
						gameStatus.scene = "Menu"; // XXX: impossible because multiplayerController is always cleaned up before this

						gameStatus?.ResetMapInfo();

						gameStatus?.ResetPerformance();

						// Release references for AfterCutScoreBuffers that don't resolve due to player leaving the map before finishing.
						noteCutMapping?.Clear();

						// Clear note id mappings.
						noteToIdMapping = null;

						statusManager?.EmitStatusUpdate(ChangedProperties.AllButNoteCut, "menu");

						if (pauseController != null) {
							pauseController.didPauseEvent -= OnGamePause;
							pauseController.didResumeEvent -= OnGameResume;
						}

						if (scoreController != null) {
							scoreController.noteWasCutEvent -= OnNoteWasCut;
							scoreController.noteWasMissedEvent -= OnNoteWasMissed;
							scoreController.scoreDidChangeEvent -= OnScoreDidChange;
							scoreController.comboDidChangeEvent -= OnComboDidChange;
							scoreController.multiplierDidChangeEvent -= OnMultiplierDidChange;
						}

						//CleanUpMultiplayer();

						if (beatmapObjectCallbackController != null) {
							beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= OnBeatmapEventDidTrigger;
						}
					}
                    catch (Exception e) {
						Plugin.log.Error(e);
                    }
				}

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~GamePlayDataManager()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async void Initialize()
        {
			try {
				gameplayCoreSceneSetupData = container.Resolve<GameplayCoreSceneSetupData>();
				pauseController = container.Resolve<PauseController>();
				scoreController = container.Resolve<ScoreController>();
				gameplayModifiers = container.Resolve<GameplayModifiers>();
				audioTimeSyncController = container.Resolve<AudioTimeSyncController>();
				beatmapObjectCallbackController = container.Resolve<BeatmapObjectCallbackController>();
				playerHeadAndObstacleInteraction = container.Resolve<PlayerHeadAndObstacleInteraction>();
				gameSongController = container.Resolve<GameSongController>();
				gameEnergyCounter = container.Resolve<GameEnergyCounter>();
				gameplayModifiersSO = this.scoreController.GetField<GameplayModifiersModelSO, ScoreController>("_gameplayModifiersModel");
			}
			catch (Exception e) {
				Plugin.log.Error(e);
				return;
			}
			Plugin.log.Info("0");

			// Check for multiplayer early to abort if needed: gameplay controllers don't exist in multiplayer until later
			gameStatus.scene = "Song";

			// FIXME: i should probably clean references to all this when song is over
			Plugin.log.Info("1");
			gameplayCoreSceneSetupData = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;

			Plugin.log.Info("2");
			Plugin.log.Info("scoreController=" + scoreController);

			// Register event listeners
			// PauseController doesn't exist in multiplayer
			if (pauseController != null) {
				Plugin.log.Info("pauseController=" + pauseController);
				// public event Action PauseController#didPauseEvent;
				pauseController.didPauseEvent += OnGamePause;
				// public event Action PauseController#didResumeEvent;
				pauseController.didResumeEvent += OnGameResume;
			}
			// public ScoreController#noteWasCutEvent<NoteData, NoteCutInfo, int multiplier> // called after AfterCutScoreBuffer is created
			scoreController.noteWasCutEvent += OnNoteWasCut;
			// public ScoreController#noteWasMissedEvent<NoteData, int multiplier>
			scoreController.noteWasMissedEvent += OnNoteWasMissed;
			// public ScoreController#scoreDidChangeEvent<int, int> // score
			scoreController.scoreDidChangeEvent += OnScoreDidChange;
			// public ScoreController#comboDidChangeEvent<int> // combo
			scoreController.comboDidChangeEvent += OnComboDidChange;
			// public ScoreController#multiplierDidChangeEvent<int, float> // multiplier, progress [0..1]
			scoreController.multiplierDidChangeEvent += OnMultiplierDidChange;
			Plugin.log.Info("2.5");
			// public event Action<BeatmapEventData> BeatmapObjectCallbackController#beatmapEventDidTriggerEvent
			beatmapObjectCallbackController.beatmapEventDidTriggerEvent += OnBeatmapEventDidTrigger;
			// public event Action GameSongController#songDidFinishEvent;
			gameSongController.songDidFinishEvent += OnLevelFinished;
			// public event Action GameEnergyCounter#gameEnergyDidReach0Event;
			gameEnergyCounter.gameEnergyDidReach0Event += OnLevelFailed;
			Plugin.log.Info("3");

			IDifficultyBeatmap diff = gameplayCoreSceneSetupData.difficultyBeatmap;
			IBeatmapLevel level = diff.level;

			gameStatus.partyMode = Gamemode.IsPartyActive;
			gameStatus.mode = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

			gameplayModifiers = gameplayCoreSceneSetupData.gameplayModifiers;
			PlayerSpecificSettings playerSettings = gameplayCoreSceneSetupData.playerSpecificSettings;
			PracticeSettings practiceSettings = gameplayCoreSceneSetupData.practiceSettings;

			float songSpeedMul = gameplayModifiers.songSpeedMul;
			if (practiceSettings != null) songSpeedMul = practiceSettings.songSpeedMul;
			float modifierMultiplier = gameplayModifiersSO.GetTotalMultiplier(gameplayModifiers);
			Plugin.log.Info("4");

			// Generate NoteData to id mappings for backwards compatiblity with <1.12.1
			noteToIdMapping = new NoteData[diff.beatmapData.cuttableNotesType + diff.beatmapData.bombsCount];
			lastNoteId = 0;
			Plugin.log.Info("4.1");

			int beatmapObjectId = 0;
			var beatmapObjectsData = diff.beatmapData.beatmapObjectsData;
			Plugin.log.Info("4.2");

			foreach (BeatmapObjectData beatmapObjectData in beatmapObjectsData) {
				if (beatmapObjectData is NoteData noteData) {
					noteToIdMapping[beatmapObjectId++] = noteData;
				}
			}
			Plugin.log.Info("5");

			gameStatus.songName = level.songName;
			gameStatus.songSubName = level.songSubName;
			gameStatus.songAuthorName = level.songAuthorName;
			gameStatus.levelAuthorName = level.levelAuthorName;
			gameStatus.songBPM = level.beatsPerMinute;
			gameStatus.noteJumpSpeed = diff.noteJumpMovementSpeed;
			// 13 is "custom_level_" and 40 is the magic number for the length of the SHA-1 hash
			gameStatus.songHash = level.levelID.StartsWith("custom_level_") && !level.levelID.EndsWith(" WIP") ? level.levelID.Substring(13, 40) : null;
			gameStatus.levelId = level.levelID;
			gameStatus.songTimeOffset = (long)(level.songTimeOffset * 1000f / songSpeedMul);
			gameStatus.length = (long)(level.beatmapLevelData.audioClip.length * 1000f / songSpeedMul);
			gameStatus.start = Utility.GetCurrentTime() - (long)(audioTimeSyncController.songTime * 1000f / songSpeedMul);
			if (practiceSettings != null) gameStatus.start -= (long)(practiceSettings.startSongTime * 1000f / songSpeedMul);
			gameStatus.paused = 0;
			gameStatus.difficulty = diff.difficulty.Name();
			gameStatus.notesCount = diff.beatmapData.cuttableNotesType;
			gameStatus.bombsCount = diff.beatmapData.bombsCount;
			gameStatus.obstaclesCount = diff.beatmapData.obstaclesCount;
			gameStatus.environmentName = level.environmentInfo.sceneInfo.sceneName;

			gameStatus.maxScore = gameplayModifiersSO.MaxModifiedScoreForMaxRawScore(ScoreModel.MaxRawScoreForNumberOfNotes(diff.beatmapData.cuttableNotesType), gameplayModifiers, gameplayModifiersSO);
			gameStatus.maxRank = RankModelHelper.MaxRankForGameplayModifiers(gameplayModifiers, gameplayModifiersSO).ToString();
			Plugin.log.Info("6");

			try {
				// From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
				var texture = (await level.GetCoverImageAsync(CancellationToken.None)).texture;
				var active = RenderTexture.active;
				var temporary = RenderTexture.GetTemporary(
					texture.width,
					texture.height,
					0,
					RenderTextureFormat.Default,
					RenderTextureReadWrite.Linear
				);

				Graphics.Blit(texture, temporary);
				RenderTexture.active = temporary;

				var cover = new Texture2D(texture.width, texture.height);
				cover.ReadPixels(new Rect(0, 0, temporary.width, temporary.height), 0, 0);
				cover.Apply();

				RenderTexture.active = active;
				RenderTexture.ReleaseTemporary(temporary);

				gameStatus.songCover = System.Convert.ToBase64String(
					ImageConversion.EncodeToPNG(cover)
				);
			}
			catch {
				gameStatus.songCover = null;
			}
			Plugin.log.Info("7");

			gameStatus.ResetPerformance();

			gameStatus.modifierMultiplier = modifierMultiplier;
			gameStatus.songSpeedMultiplier = songSpeedMul;
			gameStatus.batteryLives = gameEnergyCounter.batteryLives;

			gameStatus.modObstacles = gameplayModifiers.enabledObstacleType.ToString();
			gameStatus.modInstaFail = gameplayModifiers.instaFail;
			gameStatus.modNoFail = gameplayModifiers.noFail;
			gameStatus.modBatteryEnergy = gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery;
			gameStatus.modDisappearingArrows = gameplayModifiers.disappearingArrows;
			gameStatus.modNoBombs = gameplayModifiers.noBombs;
			gameStatus.modSongSpeed = gameplayModifiers.songSpeed.ToString();
			gameStatus.modNoArrows = gameplayModifiers.noArrows;
			gameStatus.modGhostNotes = gameplayModifiers.ghostNotes;
			gameStatus.modFailOnSaberClash = gameplayModifiers.failOnSaberClash;
			gameStatus.modStrictAngles = gameplayModifiers.strictAngles;
			gameStatus.modFastNotes = gameplayModifiers.fastNotes;

			gameStatus.staticLights = playerSettings.staticLights;
			gameStatus.leftHanded = playerSettings.leftHanded;
			gameStatus.playerHeight = playerSettings.playerHeight;
			gameStatus.sfxVolume = playerSettings.sfxVolume;
			gameStatus.reduceDebris = playerSettings.reduceDebris;
			gameStatus.noHUD = playerSettings.noTextsAndHuds;
			gameStatus.advancedHUD = playerSettings.advancedHud;
			gameStatus.autoRestart = playerSettings.autoRestart;
			Plugin.log.Info("8");

			statusManager.EmitStatusUpdate(ChangedProperties.AllButNoteCut, "songStart");
		}

		//public void CleanUpMultiplayer()
		//{
		//	if (multiplayerSessionManager != null) {
		//		multiplayerSessionManager.disconnectedEvent -= OnMultiplayerDisconnected;
		//		multiplayerSessionManager = null;
		//	}
		//}

		private static T FindFirstOrDefault<T>() where T : UnityEngine.Object
		{
			T obj = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
			if (obj == null) {
				Plugin.log.Error("Couldn't find " + typeof(T).FullName);
				throw new InvalidOperationException("Couldn't find " + typeof(T).FullName);
			}
			return obj;
		}

		private static T FindFirstOrDefaultOptional<T>() where T : UnityEngine.Object
		{
			T obj = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
			return obj;
		}

		public void OnUpdate()
		{
			bool currentHeadInObstacle = false;

			if (playerHeadAndObstacleInteraction != null) {
				currentHeadInObstacle = playerHeadAndObstacleInteraction.intersectingObstacles.Count > 0;
			}

			if (!headInObstacle && currentHeadInObstacle) {
				headInObstacle = true;

				statusManager.EmitStatusUpdate(ChangedProperties.Performance, "obstacleEnter");
			}
			else if (headInObstacle && !currentHeadInObstacle) {
				headInObstacle = false;

				statusManager.EmitStatusUpdate(ChangedProperties.Performance, "obstacleExit");
			}
		}

		//public void OnMultiplayerDisconnected(DisconnectedReason reason)
		//{
		//	CleanUpMultiplayer();

		//	// XXX: this should only be fired if we go from multiplayer lobby to menu and there's no scene transition because of it. gotta prevent duplicates too
		//	// HandleMenuStart();
		//}

		public void OnGamePause()
		{
			statusManager.gameStatus.paused = Utility.GetCurrentTime();

			statusManager.EmitStatusUpdate(ChangedProperties.Beatmap, "pause");
		}

		public void OnGameResume()
		{
			statusManager.gameStatus.start = Utility.GetCurrentTime() - (long)(audioTimeSyncController.songTime * 1000f / statusManager.gameStatus.songSpeedMultiplier);
			statusManager.gameStatus.paused = 0;

			statusManager.EmitStatusUpdate(ChangedProperties.Beatmap, "resume");
		}

		public void OnNoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
		{
			// Event order: combo, multiplier, scoreController.noteWasCut, (LateUpdate) scoreController.scoreDidChange, afterCut, (LateUpdate) scoreController.scoreDidChange

			var gameStatus = statusManager.gameStatus;

			SetNoteCutStatus(noteData, noteCutInfo, true);

			int beforeCutScore = 0;
			int afterCutScore = 0;
			int cutDistanceScore = 0;

			ScoreModel.RawScoreWithoutMultiplier(noteCutInfo, out beforeCutScore, out afterCutScore, out cutDistanceScore);

			gameStatus.initialScore = beforeCutScore + cutDistanceScore;
			gameStatus.finalScore = -1;
			gameStatus.cutDistanceScore = cutDistanceScore;
			gameStatus.cutMultiplier = multiplier;

			if (noteData.colorType == ColorType.None) {
				gameStatus.passedBombs++;
				gameStatus.hitBombs++;

				statusManager.EmitStatusUpdate(ChangedProperties.PerformanceAndNoteCut, "bombCut");
			}
			else {
				gameStatus.passedNotes++;

				if (noteCutInfo.allIsOK) {
					gameStatus.hitNotes++;

					statusManager.EmitStatusUpdate(ChangedProperties.PerformanceAndNoteCut, "noteCut");
				}
				else {
					gameStatus.missedNotes++;

					statusManager.EmitStatusUpdate(ChangedProperties.PerformanceAndNoteCut, "noteMissed");
				}
			}

			List<CutScoreBuffer> list = (List<CutScoreBuffer>)afterCutScoreBuffersField.GetValue(scoreController);

			foreach (CutScoreBuffer acsb in list) {
				if (noteCutInfoField.GetValue(acsb) == noteCutInfo && noteCutMapping.TryAdd(noteCutInfo, noteData)) {
					// public CutScoreBuffer#didFinishEvent<CutScoreBuffer>
					acsb.didFinishEvent += OnNoteWasFullyCut;
					break;
				}
			}
		}

		public void OnNoteWasFullyCut(CutScoreBuffer acsb)
		{
			int beforeCutScore;
			int afterCutScore;
			int cutDistanceScore;

			NoteCutInfo noteCutInfo = (NoteCutInfo)noteCutInfoField.GetValue(acsb);

            if (noteCutMapping.TryRemove(noteCutInfo, out var noteData)) {
				SetNoteCutStatus(noteData, noteCutInfo, false);
			}
			// public static ScoreModel.RawScoreWithoutMultiplier(NoteCutInfo, out int beforeCutRawScore, out int afterCutRawScore, out int cutDistanceRawScore)
			ScoreModel.RawScoreWithoutMultiplier(noteCutInfo, out beforeCutScore, out afterCutScore, out cutDistanceScore);

			int multiplier = (int)cutScoreBufferMultiplierField.GetValue(acsb);

			statusManager.gameStatus.initialScore = beforeCutScore + cutDistanceScore;
			statusManager.gameStatus.finalScore = beforeCutScore + afterCutScore + cutDistanceScore;
			statusManager.gameStatus.cutDistanceScore = cutDistanceScore;
			statusManager.gameStatus.cutMultiplier = multiplier;

			statusManager.EmitStatusUpdate(ChangedProperties.PerformanceAndNoteCut, "noteFullyCut");

			acsb.didFinishEvent -= OnNoteWasFullyCut;
		}

		private void SetNoteCutStatus(NoteData noteData, NoteCutInfo noteCutInfo = null, bool initialCut = true)
		{
			GameStatus gameStatus = statusManager.gameStatus;

			gameStatus.ResetNoteCut();

			// Backwards compatibility for <1.12.1
			gameStatus.noteID = -1;
			// Check the near notes first for performance
			for (int i = Math.Max(0, lastNoteId - 10); i < noteToIdMapping.Length; i++) {
				if (Utility.NoteDataEquals(noteToIdMapping[i], noteData)) {
					gameStatus.noteID = i;
					if (i > lastNoteId) lastNoteId = i;
					break;
				}
			}
			// If that failed, check the rest of the notes in reverse order
			if (gameStatus.noteID == -1) {
				for (int i = Math.Max(0, lastNoteId - 11); i >= 0; i--) {
					if (Utility.NoteDataEquals(noteToIdMapping[i], noteData)) {
						gameStatus.noteID = i;
						break;
					}
				}
			}

			// Backwards compatibility for <1.12.1
			gameStatus.noteType = noteData.colorType == ColorType.None ? "Bomb" : noteData.colorType == ColorType.ColorA ? "NoteA" : noteData.colorType == ColorType.ColorB ? "NoteB" : noteData.colorType.ToString();
			gameStatus.noteCutDirection = noteData.cutDirection.ToString();
			gameStatus.noteLine = noteData.lineIndex;
			gameStatus.noteLayer = (int)noteData.noteLineLayer;
			// If long notes are ever introduced, this name will make no sense
			gameStatus.timeToNextBasicNote = noteData.timeToNextColorNote;

			if (noteCutInfo != null) {
				gameStatus.speedOK = noteCutInfo.speedOK;
				gameStatus.directionOK = noteCutInfo.directionOK;
				gameStatus.saberTypeOK = noteCutInfo.saberTypeOK;
				gameStatus.wasCutTooSoon = noteCutInfo.wasCutTooSoon;
				gameStatus.saberSpeed = noteCutInfo.saberSpeed;
				gameStatus.saberDirX = noteCutInfo.saberDir[0];
				gameStatus.saberDirY = noteCutInfo.saberDir[1];
				gameStatus.saberDirZ = noteCutInfo.saberDir[2];
				gameStatus.saberType = noteCutInfo.saberType.ToString();
				gameStatus.swingRating = noteCutInfo.swingRatingCounter == null ? -1 : initialCut ? noteCutInfo.swingRatingCounter.beforeCutRating : noteCutInfo.swingRatingCounter.afterCutRating;
				gameStatus.timeDeviation = noteCutInfo.timeDeviation;
				gameStatus.cutDirectionDeviation = noteCutInfo.cutDirDeviation;
				gameStatus.cutPointX = noteCutInfo.cutPoint[0];
				gameStatus.cutPointY = noteCutInfo.cutPoint[1];
				gameStatus.cutPointZ = noteCutInfo.cutPoint[2];
				gameStatus.cutNormalX = noteCutInfo.cutNormal[0];
				gameStatus.cutNormalY = noteCutInfo.cutNormal[1];
				gameStatus.cutNormalZ = noteCutInfo.cutNormal[2];
				gameStatus.cutDistanceToCenter = noteCutInfo.cutDistanceToCenter;
			}
		}

		public void OnNoteWasMissed(NoteData noteData, int multiplier)
		{
			// Event order: combo, multiplier, scoreController.noteWasMissed, (LateUpdate) scoreController.scoreDidChange

			statusManager.gameStatus.batteryEnergy = gameEnergyCounter.batteryEnergy;

			SetNoteCutStatus(noteData);

			if (noteData.colorType == ColorType.None) {
				statusManager.gameStatus.passedBombs++;

				statusManager.EmitStatusUpdate(ChangedProperties.PerformanceAndNoteCut, "bombMissed");
			}
			else {
				statusManager.gameStatus.passedNotes++;
				statusManager.gameStatus.missedNotes++;

				statusManager.EmitStatusUpdate(ChangedProperties.PerformanceAndNoteCut, "noteMissed");
			}
		}

		public void OnScoreDidChange(int scoreBeforeMultiplier, int scoreAfterMultiplier)
		{
			GameStatus gameStatus = statusManager.gameStatus;

			gameStatus.score = scoreAfterMultiplier;

			int currentMaxScoreBeforeMultiplier = ScoreModel.MaxRawScoreForNumberOfNotes(gameStatus.passedNotes);
			gameStatus.currentMaxScore = gameplayModifiersSO.MaxModifiedScoreForMaxRawScore(currentMaxScoreBeforeMultiplier, gameplayModifiers, gameplayModifiersSO);

			RankModel.Rank rank = RankModel.GetRankForScore(scoreBeforeMultiplier, gameStatus.score, currentMaxScoreBeforeMultiplier, gameStatus.currentMaxScore);
			gameStatus.rank = RankModel.GetRankName(rank);

			statusManager.EmitStatusUpdate(ChangedProperties.Performance, "scoreChanged");
		}

		public void OnComboDidChange(int combo)
		{
			statusManager.gameStatus.combo = combo;
			// public int ScoreController#maxCombo
			statusManager.gameStatus.maxCombo = scoreController.maxCombo;
		}

		public void OnMultiplierDidChange(int multiplier, float multiplierProgress)
		{
			statusManager.gameStatus.multiplier = multiplier;
			statusManager.gameStatus.multiplierProgress = multiplierProgress;
		}

		public void OnLevelFinished()
		{
			statusManager.EmitStatusUpdate(ChangedProperties.Performance, "finished");
		}

		public void OnLevelFailed()
		{
			statusManager.EmitStatusUpdate(ChangedProperties.Performance, "failed");
		}

		public void OnBeatmapEventDidTrigger(BeatmapEventData beatmapEventData)
		{
			statusManager.gameStatus.beatmapEventType = (int)beatmapEventData.type;
			statusManager.gameStatus.beatmapEventValue = beatmapEventData.value;

			statusManager.EmitStatusUpdate(ChangedProperties.BeatmapEvent, "beatmapEvent");
		}

		

		
	}
}
