using BS_Utils.Gameplay;
using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using IPA.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Zenject;

namespace HttpSiraStatus.Models
{
    public class GamePlayDataManager : MonoBehaviour, IInitializable, IDisposable, ICutScoreBufferDidFinishEvent
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer)
        {
            cutScoreBuffer.didFinishEvent.Remove(this);
            if (cutScoreBuffer is CustomCutBuffer customCutBuffer) {
                var noteCutInfo = customCutBuffer.NoteCutInfo;

                if (this.noteCutMapping.TryRemove(noteCutInfo, out var noteData)) {
                    this.SetNoteCutStatus(noteData, noteCutInfo, false);
                }
                // public static ScoreModel.RawScoreWithoutMultiplier(NoteCutInfo, out int beforeCutRawScore, out int afterCutRawScore, out int cutDistanceRawScore)
                ScoreModel.RawScoreWithoutMultiplier(noteCutInfo.swingRatingCounter, noteCutInfo.cutDistanceToCenter, out var beforeCutScore, out var afterCutScore, out var cutDistanceScore);

                var multiplier = customCutBuffer.multiplier;

                this.statusManager.GameStatus.initialScore = beforeCutScore + cutDistanceScore;
                this.statusManager.GameStatus.finalScore = beforeCutScore + afterCutScore + cutDistanceScore;
                this.statusManager.GameStatus.cutDistanceScore = cutDistanceScore;
                this.statusManager.GameStatus.cutMultiplier = multiplier;

                this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteFullyCut);
                this.activeItems.Remove(customCutBuffer);
                this.cutBufferPool.Despawn(customCutBuffer);
            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnMultiplayerLevelFinished(MultiplayerLevelCompletionResults obj)
        {
            switch (obj.levelEndState) {
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.Cleared:
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.GivenUp:
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.WasInactive:
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.HostEndedLevel:
                    this.OnLevelFinished();
                    break;
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.Failed:
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.StartupFailed:
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.ConnectedAfterLevelEnded:
                case MultiplayerLevelCompletionResults.MultiplayerLevelEndState.Quit:
                    this.OnLevelFailed();
                    break;
                default:
                    this.OnLevelFinished();
                    break;
            }
        }

        private void OnImmediateMaxPossibleScoreDidChangeEvent(int immediateMaxPossibleScore, int immediateMaxPossibleModifiedScore)
        {
            this.statusManager.GameStatus.currentMaxScore = immediateMaxPossibleModifiedScore;
            this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent()
        {
            this.gameStatus.relativeScore = this.relativeScoreAndImmediateRankCounter.relativeScore;
            this.gameStatus.rank = RankModel.GetRankName(this.relativeScoreAndImmediateRankCounter.immediateRank);
            this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }
        private void OnEnergyChanged(float obj)
        {
            this.gameStatus.energy = obj;
            this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.EnergyChanged);
        }
        /// <summary>
        /// こいつ別スレで呼ぶと悲惨なことになるのでUpdateで呼ぶことにしました。
        /// </summary>
        private void OnObstacleInteraction()
        {
            if (this.playerHeadAndObstacleInteraction == null) {
                return;
            }
            // intersectingObstaclesのgetがフレーム単位で呼ばないといけない。
            // 別スレで呼ぶと関係ないとこで他のモジュール群が死ぬ。
            var intersectingObstacles = this.playerHeadAndObstacleInteraction.intersectingObstacles;
            var currentHeadInObstacle = intersectingObstacles.Any();

            if (!this.headInObstacle && currentHeadInObstacle) {
                this.headInObstacle = true;
                this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ObstacleEnter);
            }
            else if (this.headInObstacle && !currentHeadInObstacle) {
                this.headInObstacle = false;
                this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ObstacleExit);
            }
        }

        private void UpdateCurrentSongTime()
        {
            var songTime = Mathf.FloorToInt(this.audioTimeSource.songTime);
            if (this.statusManager.GameStatus.currentSongTime != songTime) {
                this.statusManager.GameStatus.currentSongTime = songTime;
                this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.BeatmapEvent);
            }
        }

        private void UpdateModMultiplier()
        {
            var gameStatus = this.statusManager.GameStatus;

            var energy = this.gameEnergyCounter.energy;

            gameStatus.modifierMultiplier = this.gameplayModifiersSO.GetTotalMultiplier(this.gameplayModifiersSO.CreateModifierParamsList(this.gameplayModifiers), energy);

            gameStatus.maxScore = this.gameplayModifiersSO.MaxModifiedScoreForMaxRawScore(ScoreModel.MaxRawScoreForNumberOfNotes(this.gameplayCoreSceneSetupData.difficultyBeatmap.beatmapData.cuttableNotesCount), this.gameplayModifiersSO.CreateModifierParamsList(this.gameplayModifiers), this.gameplayModifiersSO, energy);
            gameStatus.maxRank = RankModelHelper.MaxRankForGameplayModifiers(this.gameplayModifiers, this.gameplayModifiersSO, energy).ToString();
        }

        private void OnGamePause()
        {
            this.statusManager.GameStatus.paused = Utility.GetCurrentTime();

            this.statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Pause);
        }

        private void OnGameResume()
        {
            this.statusManager.GameStatus.start = Utility.GetCurrentTime() - (long)(this.audioTimeSource.songTime * 1000f / this.statusManager.GameStatus.songSpeedMultiplier);
            this.statusManager.GameStatus.paused = 0;

            this.statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Resume);
        }

        private void OnNoteWasCut(NoteData noteData, in NoteCutInfo noteCutInfo, int multiplier)
        {
            // Event order: combo, multiplier, scoreController.noteWasCut, (LateUpdate) scoreController.scoreDidChange, afterCut, (LateUpdate) scoreController.scoreDidChange

            var gameStatus = this.statusManager.GameStatus;

            this.SetNoteCutStatus(noteData, noteCutInfo, true);

            //int beforeCutScore = 0;
            //int afterCutScore = 0;
            //int cutDistanceScore = 0;

            ScoreModel.RawScoreWithoutMultiplier(noteCutInfo.swingRatingCounter, noteCutInfo.cutDistanceToCenter, out var beforeCutScore, out _, out var cutDistanceScore);

            gameStatus.initialScore = beforeCutScore + cutDistanceScore;
            gameStatus.finalScore = -1;
            gameStatus.cutDistanceScore = cutDistanceScore;
            gameStatus.cutMultiplier = multiplier;

            if (noteData.colorType == ColorType.None) {
                gameStatus.passedBombs++;
                gameStatus.hitBombs++;

                this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.BombCut);
            }
            else {
                gameStatus.passedNotes++;

                if (noteCutInfo.allIsOK) {
                    gameStatus.hitNotes++;

                    this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteCut);
                }
                else {
                    gameStatus.missedNotes++;

                    this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteMissed);
                }
            }

            if (this.noteCutMapping.TryRemove(noteCutInfo, out var changeNoteData)) {
                this.SetNoteCutStatus(changeNoteData, noteCutInfo, false);
            }
            this.statusManager.GameStatus.initialScore = beforeCutScore + cutDistanceScore;
            this.statusManager.GameStatus.cutDistanceScore = cutDistanceScore;
            this.statusManager.GameStatus.cutMultiplier = multiplier;
            this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteFullyCut);
            if (noteCutInfo.allIsOK && this.noteCutMapping.TryAdd(noteCutInfo, noteData)) {
                this.activeItems.Add(this.cutBufferPool.Spawn(noteCutInfo, multiplier, this));
            }
        }

        
        private void SetNoteCutStatus(NoteData noteData, NoteCutInfo noteCutInfo = default, bool initialCut = true)
        {
            var gameStatus = this.statusManager.GameStatus;

            gameStatus.ResetNoteCut();

            // Backwards compatibility for <1.12.1
            gameStatus.noteID = -1;
            // Check the near notes first for performance
            var entiy = this.notePool.Spawn(noteData, this.gameplayModifiers.noArrows);
            if (this._noteToIdMapping.TryRemove(entiy, out var noteID)) {
                gameStatus.noteID = noteID;
                if (this.lastNoteId < noteID) {
                    this.lastNoteId = noteID;
                }
            }
            else {
                gameStatus.noteID = this.lastNoteId;
            }
            this.notePool.Despawn(entiy);
            // Backwards compatibility for <1.12.1
            gameStatus.noteType = noteData.colorType == ColorType.None ? "Bomb" : noteData.colorType == ColorType.ColorA ? "NoteA" : noteData.colorType == ColorType.ColorB ? "NoteB" : noteData.colorType.ToString();
            gameStatus.noteCutDirection = noteData.cutDirection.ToString();
            gameStatus.noteLine = noteData.lineIndex;
            gameStatus.noteLayer = (int)noteData.noteLineLayer;
            // If long notes are ever introduced, this name will make no sense
            gameStatus.timeToNextBasicNote = noteData.timeToNextColorNote;

            if (!EqualityComparer<NoteCutInfo>.Default.Equals(noteCutInfo, default)) {
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

        private void OnNoteWasMissed(NoteData noteData, int multiplier)
        {
            // Event order: combo, multiplier, scoreController.noteWasMissed, (LateUpdate) scoreController.scoreDidChange

            this.statusManager.GameStatus.batteryEnergy = this.gameEnergyCounter.batteryEnergy;
            this.statusManager.GameStatus.energy = this.gameEnergyCounter.energy;

            this.SetNoteCutStatus(noteData);

            if (noteData.colorType == ColorType.None) {
                this.statusManager.GameStatus.passedBombs++;

                this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.BombMissed);
            }
            else {
                this.statusManager.GameStatus.passedNotes++;
                this.statusManager.GameStatus.missedNotes++;

                this.statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteMissed);
            }
        }

        private void OnScoreDidChange(int scoreBeforeMultiplier, int scoreAfterMultiplier)
        {
            var gameStatus = this.statusManager.GameStatus;

            gameStatus.rawScore = scoreBeforeMultiplier;
            gameStatus.score = scoreAfterMultiplier;
            this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }
        private void OnComboDidChange(int combo)
        {
            this.statusManager.GameStatus.combo = combo;
            // public int ScoreController#maxCombo
            this.statusManager.GameStatus.maxCombo = this.scoreController.maxCombo;
        }

        private void OnMultiplierDidChange(int multiplier, float multiplierProgress)
        {
            this.statusManager.GameStatus.multiplier = multiplier;
            this.statusManager.GameStatus.multiplierProgress = multiplierProgress;
        }

        private void OnLevelFinished() => this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.Finished);

        private void OnLevelFailed() => this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.Failed);

        private void OnEnergyDidReach0Event()
        {
            if (this.statusManager.GameStatus.modNoFail) {
                this.statusManager.GameStatus.softFailed = true;
                this.UpdateModMultiplier();
                this.statusManager.EmitStatusUpdate(ChangedProperty.BeatmapAndPerformanceAndMod, BeatSaberEvent.SoftFailed);
            }
        }

        private void OnBeatmapEventDidTrigger(BeatmapEventData beatmapEventData)
        {
            this.statusManager.GameStatus.beatmapEventType = (int)beatmapEventData.type;
            this.statusManager.GameStatus.beatmapEventValue = beatmapEventData.value;

            this.statusManager.EmitStatusUpdate(ChangedProperty.BeatmapEvent, BeatSaberEvent.BeatmapEvent);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private IStatusManager statusManager;
        private GameStatus gameStatus;
        private CustomCutBuffer.Pool cutBufferPool;
        private NoteDataEntity.Pool notePool;
        private GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
        private PauseController pauseController;
        private IScoreController scoreController;
        private GameplayModifiers gameplayModifiers;
        private IAudioTimeSource audioTimeSource;
        private BeatmapObjectCallbackController beatmapObjectCallbackController;
        private PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction;
        private GameEnergyCounter gameEnergyCounter;
        private MultiplayerLocalActivePlayerFacade multiplayerLocalActivePlayerFacade;
        private RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter;
        private ILevelEndActions levelEndActions;
        private readonly ConcurrentDictionary<NoteCutInfo, NoteData> noteCutMapping = new ConcurrentDictionary<NoteCutInfo, NoteData>();
        private GameplayModifiersModelSO gameplayModifiersSO;
        private readonly LazyCopyHashSet<CustomCutBuffer> activeItems = new LazyCopyHashSet<CustomCutBuffer>();
        private bool headInObstacle = false;
        private Thread _thread;
        private int lastNoteId = 0;
        private bool disposedValue;
        /// <summary>
        /// Beat Saber 1.12.1 removes NoteData.id, forcing us to generate our own note IDs to allow users to easily link events about the same note.
        /// Before 1.12.1 the noteID matched the note order in the beatmap file, but this is impossible to replicate now without hooking into the level loading code.
        /// </summary>
        private readonly ConcurrentDictionary<NoteDataEntity, int> _noteToIdMapping = new ConcurrentDictionary<NoteDataEntity, int>();
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        /// <summary>
        /// 引数やっば
        /// </summary>
        /// <param name="statusManager"></param>
        /// <param name="gameStatus"></param>
        /// <param name="customCutBufferPool"></param>
        /// <param name="noteEntityPool"></param>
        /// <param name="gameplayCoreSceneSetupData"></param>
        /// <param name="score"></param>
        /// <param name="gameplayModifiers"></param>
        /// <param name="audioTimeSource"></param>
        /// <param name="beatmapObjectCallbackController"></param>
        /// <param name="playerHeadAndObstacleInteraction"></param>
        /// <param name="gameEnergyCounter"></param>
        /// <param name="relative"></param>
        /// <param name="diContainer"></param>
        [Inject]
        private void Constractor(
            IStatusManager statusManager,
            GameStatus gameStatus,
            CustomCutBuffer.Pool customCutBufferPool,
            NoteDataEntity.Pool noteEntityPool,
            GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            IScoreController score,
            GameplayModifiers gameplayModifiers,
            IAudioTimeSource audioTimeSource,
            BeatmapObjectCallbackController beatmapObjectCallbackController,
            PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction,
            GameEnergyCounter gameEnergyCounter,
            RelativeScoreAndImmediateRankCounter relative,
            DiContainer diContainer)
        {
            this.statusManager = statusManager;
            this.gameStatus = gameStatus;
            this.cutBufferPool = customCutBufferPool;
            this.notePool = noteEntityPool;
            this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this.scoreController = score;
            this.gameplayModifiers = gameplayModifiers;
            this.audioTimeSource = audioTimeSource;
            this.beatmapObjectCallbackController = beatmapObjectCallbackController;
            this.playerHeadAndObstacleInteraction = playerHeadAndObstacleInteraction;
            this.gameEnergyCounter = gameEnergyCounter;
            this.relativeScoreAndImmediateRankCounter = relative;
            if (this.scoreController is ScoreController scoreController) {
                this.gameplayModifiersSO = scoreController.GetField<GameplayModifiersModelSO, ScoreController>("_gameplayModifiersModel");
            }
            this.pauseController = diContainer.TryResolve<PauseController>();
            this.levelEndActions = diContainer.TryResolve<ILevelEndActions>();
            this.multiplayerLocalActivePlayerFacade = diContainer.TryResolve<MultiplayerLocalActivePlayerFacade>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue) {
                if (disposing) {
                    Plugin.Logger.Debug("dispose call");
                    try {
                        this.gameStatus.scene = "Menu"; // XXX: impossible because multiplayerController is always cleaned up before this

                        this.gameStatus?.ResetMapInfo();

                        this.gameStatus?.ResetPerformance();

                        // Release references for AfterCutScoreBuffers that don't resolve due to player leaving the map before finishing.
                        this.noteCutMapping?.Clear();
                        while (this.activeItems.items.Any()) {
                            var note = this.activeItems.items.First();
                            this.activeItems.items.RemoveAt(0);
                            this.cutBufferPool.Despawn(note);
                        }
                        // Clear note id mappings.
                        this._noteToIdMapping?.Clear();

                        this.statusManager?.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.Menu);

                        this._thread?.Abort();

                        if (this.pauseController != null) {
                            this.pauseController.didPauseEvent -= this.OnGamePause;
                            this.pauseController.didResumeEvent -= this.OnGameResume;
                        }

                        if (this.scoreController != null) {
                            this.scoreController.noteWasCutEvent -= this.OnNoteWasCut;
                            this.scoreController.noteWasMissedEvent -= this.OnNoteWasMissed;
                            this.scoreController.scoreDidChangeEvent -= this.OnScoreDidChange;
                            this.scoreController.comboDidChangeEvent -= this.OnComboDidChange;
                            this.scoreController.multiplierDidChangeEvent -= this.OnMultiplierDidChange;
                            this.scoreController.immediateMaxPossibleScoreDidChangeEvent -= this.OnImmediateMaxPossibleScoreDidChangeEvent;
                        }

                        if (this.multiplayerLocalActivePlayerFacade != null) {
                            this.multiplayerLocalActivePlayerFacade.playerDidFinishEvent -= this.OnMultiplayerLevelFinished;
                            this.multiplayerLocalActivePlayerFacade = null;
                        }

                        if (this.levelEndActions != null) {
                            this.levelEndActions.levelFinishedEvent -= this.OnLevelFinished;
                            this.levelEndActions.levelFailedEvent -= this.OnLevelFailed;
                        }

                        if (this.beatmapObjectCallbackController != null) {
                            this.beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= this.OnBeatmapEventDidTrigger;
                        }

                        if (this.gameEnergyCounter != null) {
                            this.gameEnergyCounter.gameEnergyDidChangeEvent -= this.OnEnergyChanged;
                            this.gameEnergyCounter.gameEnergyDidReach0Event -= this.OnEnergyDidReach0Event;
                        }
                        if (this.relativeScoreAndImmediateRankCounter) {
                            this.relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
                        }
                    }
                    catch (Exception e) {
                        Plugin.Logger.Error(e);
                    }
                }
                this.disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async void Initialize()
        {
            Plugin.Logger.Info("Initialize()");
            // Check for multiplayer early to abort if needed: gameplay controllers don't exist in multiplayer until later
            this.gameStatus.scene = "Song";
            // Register event listeners
            // PauseController doesn't exist in multiplayer
            if (this.pauseController != null) {
                // public event Action PauseController#didPauseEvent;
                this.pauseController.didPauseEvent += this.OnGamePause;
                // public event Action PauseController#didResumeEvent;
                this.pauseController.didResumeEvent += this.OnGameResume;
            }
            // public ScoreController#noteWasCutEvent<NoteData, NoteCutInfo, int multiplier> // called after AfterCutScoreBuffer is created
            this.scoreController.noteWasCutEvent += this.OnNoteWasCut;
            // public ScoreController#noteWasMissedEvent<NoteData, int multiplier>
            this.scoreController.noteWasMissedEvent += this.OnNoteWasMissed;
            // public ScoreController#scoreDidChangeEvent<int, int> // score
            this.scoreController.scoreDidChangeEvent += this.OnScoreDidChange;
            // public ScoreController#comboDidChangeEvent<int> // combo
            this.scoreController.comboDidChangeEvent += this.OnComboDidChange;
            // public ScoreController#multiplierDidChangeEvent<int, float> // multiplier, progress [0..1]
            this.scoreController.multiplierDidChangeEvent += this.OnMultiplierDidChange;
            this.scoreController.immediateMaxPossibleScoreDidChangeEvent += this.OnImmediateMaxPossibleScoreDidChangeEvent;
            // public event Action<BeatmapEventData> BeatmapObjectCallbackController#beatmapEventDidTriggerEvent
            this.beatmapObjectCallbackController.beatmapEventDidTriggerEvent += this.OnBeatmapEventDidTrigger;
            this.relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
            // public event Action GameEnergyCounter#gameEnergyDidReach0Event;
            this.gameEnergyCounter.gameEnergyDidReach0Event += this.OnEnergyDidReach0Event;
            this.gameEnergyCounter.gameEnergyDidChangeEvent += this.OnEnergyChanged;

            if (this.multiplayerLocalActivePlayerFacade != null) {
                this.multiplayerLocalActivePlayerFacade.playerDidFinishEvent += this.OnMultiplayerLevelFinished;
            }
            if (this.levelEndActions != null) {
                this.levelEndActions.levelFinishedEvent += this.OnLevelFinished;
                this.levelEndActions.levelFailedEvent += this.OnLevelFailed;
            }
            var diff = this.gameplayCoreSceneSetupData.difficultyBeatmap;
            var level = diff.level;

            this.gameStatus.partyMode = Gamemode.IsPartyActive;
            this.gameStatus.mode = this.gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

            this.gameplayModifiers = this.gameplayCoreSceneSetupData.gameplayModifiers;
            var playerSettings = this.gameplayCoreSceneSetupData.playerSpecificSettings;
            var practiceSettings = this.gameplayCoreSceneSetupData.practiceSettings;

            var songSpeedMul = this.gameplayModifiers.songSpeedMul;
            if (practiceSettings != null) {
                songSpeedMul = practiceSettings.songSpeedMul;
            }
            // Generate NoteData to id mappings for backwards compatiblity with <1.12.1
            this._noteToIdMapping.Clear();

            this.lastNoteId = 0;
            foreach (var note in diff.beatmapData.beatmapObjectsData.Where(x => x is NoteData).Select((x, i) => new { note = x, index = i })) {
                this._noteToIdMapping.TryAdd(new NoteDataEntity(note.note as NoteData, this.gameplayModifiers.noArrows), note.index);
            }
            this.gameStatus.songName = level.songName;
            this.gameStatus.songSubName = level.songSubName;
            this.gameStatus.songAuthorName = level.songAuthorName;
            this.gameStatus.levelAuthorName = level.levelAuthorName;
            this.gameStatus.songBPM = level.beatsPerMinute;
            this.gameStatus.noteJumpSpeed = diff.noteJumpMovementSpeed;
            // 13 is "custom_level_" and 40 is the magic number for the length of the SHA-1 hash
            this.gameStatus.songHash = Regex.IsMatch(level.levelID, "^custom_level_[0-9A-F]{40}", RegexOptions.IgnoreCase) && !level.levelID.EndsWith(" WIP") ? level.levelID.Substring(13, 40) : null;
            this.gameStatus.levelId = level.levelID;
            this.gameStatus.songTimeOffset = (long)(level.songTimeOffset * 1000f / songSpeedMul);
            this.gameStatus.length = (long)(level.beatmapLevelData.audioClip.length * 1000f / songSpeedMul);
            this.gameStatus.start = Utility.GetCurrentTime() - (long)(this.audioTimeSource.songTime * 1000f / songSpeedMul);
            if (practiceSettings != null)
                this.gameStatus.start -= (long)(practiceSettings.startSongTime * 1000f / songSpeedMul);
            this.gameStatus.paused = 0;
            this.gameStatus.difficulty = diff.difficulty.Name();
            this.gameStatus.difficultyEnum = Enum.GetName(typeof(BeatmapDifficulty), diff.difficulty);
            this.gameStatus.notesCount = diff.beatmapData.cuttableNotesCount;
            this.gameStatus.bombsCount = diff.beatmapData.bombsCount;
            this.gameStatus.obstaclesCount = diff.beatmapData.obstaclesCount;
            this.gameStatus.environmentName = level.environmentInfo.sceneInfo.sceneName;
            var colorScheme = gameplayCoreSceneSetupData.colorScheme ?? new ColorScheme(gameplayCoreSceneSetupData.environmentInfo.colorScheme);
            gameStatus.colorSaberA = colorScheme.saberAColor;
            gameStatus.colorSaberB = colorScheme.saberBColor;
            gameStatus.colorEnvironment0 = colorScheme.environmentColor0;
            gameStatus.colorEnvironment1 = colorScheme.environmentColor1;
            if (colorScheme.supportsEnvironmentColorBoost) {
                gameStatus.colorEnvironmentBoost0 = colorScheme.environmentColor0Boost;
                gameStatus.colorEnvironmentBoost1 = colorScheme.environmentColor1Boost;
            }
            gameStatus.colorObstacle = colorScheme.obstaclesColor;
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

                this.gameStatus.songCover = System.Convert.ToBase64String(
                    ImageConversion.EncodeToPNG(cover)
                );
            }
            catch (Exception e) {
                Plugin.Logger.Error(e);
                this.gameStatus.songCover = null;
            }

            this.UpdateModMultiplier();
            this.gameStatus.songSpeedMultiplier = songSpeedMul;
            this.gameStatus.batteryLives = this.gameEnergyCounter.batteryLives;

            this.gameStatus.modObstacles = this.gameplayModifiers.enabledObstacleType.ToString();
            this.gameStatus.modInstaFail = this.gameplayModifiers.instaFail;
            this.gameStatus.modNoFail = this.gameplayModifiers.noFailOn0Energy;
            this.gameStatus.modBatteryEnergy = this.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery;
            this.gameStatus.modDisappearingArrows = this.gameplayModifiers.disappearingArrows;
            this.gameStatus.modNoBombs = this.gameplayModifiers.noBombs;
            this.gameStatus.modSongSpeed = this.gameplayModifiers.songSpeed.ToString();
            this.gameStatus.modNoArrows = this.gameplayModifiers.noArrows;
            this.gameStatus.modGhostNotes = this.gameplayModifiers.ghostNotes;
            this.gameStatus.modFailOnSaberClash = this.gameplayModifiers.failOnSaberClash;
            this.gameStatus.modStrictAngles = this.gameplayModifiers.strictAngles;
            this.gameStatus.modFastNotes = this.gameplayModifiers.fastNotes;
            this.gameStatus.modSmallNotes = this.gameplayModifiers.smallCubes;
            this.gameStatus.modProMode = this.gameplayModifiers.proMode;
            this.gameStatus.modZenMode = this.gameplayModifiers.zenMode;

            this.gameStatus.staticLights = (diff.difficulty == BeatmapDifficulty.ExpertPlus ? playerSettings.environmentEffectsFilterExpertPlusPreset : playerSettings.environmentEffectsFilterDefaultPreset) != EnvironmentEffectsFilterPreset.AllEffects;
            this.gameStatus.leftHanded = playerSettings.leftHanded;
            this.gameStatus.playerHeight = playerSettings.playerHeight;
            this.gameStatus.sfxVolume = playerSettings.sfxVolume;
            this.gameStatus.reduceDebris = playerSettings.reduceDebris;
            this.gameStatus.noHUD = playerSettings.noTextsAndHuds;
            this.gameStatus.advancedHUD = playerSettings.advancedHud;
            this.gameStatus.autoRestart = playerSettings.autoRestart;
            this.gameStatus.saberTrailIntensity = playerSettings.saberTrailIntensity;
            this.gameStatus.environmentEffects = (diff.difficulty == BeatmapDifficulty.ExpertPlus ? playerSettings.environmentEffectsFilterExpertPlusPreset : playerSettings.environmentEffectsFilterDefaultPreset).ToString();
            this.gameStatus.hideNoteSpawningEffect = playerSettings.hideNoteSpawnEffect;

            this._thread = new Thread(new ThreadStart(() =>
            {
                while (!this.disposedValue) {
                    try {
                        this.UpdateCurrentSongTime();
                    }
                    catch (Exception e) {
                        Plugin.Logger.Error(e);
                    }
                    finally {
                        Thread.Sleep(16);
                    }
                }
            }));
            this._thread.Start();
            this.statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // Unity Method
        private void Update()
        {
            try {
                this.OnObstacleInteraction();
            }
            catch (Exception e) {
                Plugin.Logger.Error(e);
            }
        }
        #endregion
    }
}
