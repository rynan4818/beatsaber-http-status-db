using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Zenject;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace HttpSiraStatus.Models
{
    public class GamePlayDataManager : MonoBehaviour, IAsyncInitializable, IDisposable, ICutScoreBufferDidFinishReceiver, IAffinity
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer)
        {
            cutScoreBuffer.UnregisterDidFinishReceiver(this);
            if (cutScoreBuffer is CustomCutBuffer customCutBuffer) {
                var noteCutInfo = customCutBuffer.NoteCutInfo;
                this.SetNoteCutStatus(customCutBuffer.NoteController, noteCutInfo, false);
                this._gameStatus.swingRating = cutScoreBuffer.noteCutInfo.saberMovementData.ComputeSwingRating();
                this._gameStatus.afterSwingRating = cutScoreBuffer.afterCutSwingRating;
                this._gameStatus.beforSwingRating = cutScoreBuffer.beforeCutSwingRating;
                this._gameStatus.cutMultiplier = this._multiplier;

                this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteFullyCut);
                this._cutBufferPool.Despawn(customCutBuffer);
            }
        }

        [AffinityPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasCut))]
        [AffinityPostfix]
        public void NoteWasCutPostfix(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteController.noteData.scoringType == NoteData.ScoringType.Ignore) {
                return;
            }
            this.OnNoteWasCutEvent(noteController, noteCutInfo);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnMultiplayerLevelFinished(MultiplayerLevelCompletionResults obj)
        {
            switch (obj.playerLevelEndReason) {
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.Cleared:
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.GivenUp:
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.WasInactive:
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.HostEndedLevel:
                    this.OnLevelFinished();
                    break;
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.Failed:
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.Quit:
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.StartupFailed:
                case MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndReason.ConnectedAfterLevelEnded:
                    this.OnLevelFailed();
                    break;
                default:
                    this.OnLevelFinished();
                    break;
            }
        }

        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent()
        {
            this._gameStatus.relativeScore = this._relativeScoreAndImmediateRankCounter.relativeScore;
            this._gameStatus.rank = RankModel.GetRankName(this._relativeScoreAndImmediateRankCounter.immediateRank);
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }
        private void OnEnergyChanged(float obj)
        {
            this._gameStatus.energy = obj;
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.EnergyChanged);
        }
        /// <summary>
        /// こいつ別スレで呼ぶと悲惨なことになるのでUpdateで呼ぶことにしました。
        /// </summary>
        private void OnObstacleInteraction()
        {
            if (this._playerHeadAndObstacleInteraction == null) {
                return;
            }
            // intersectingObstaclesのgetがフレーム単位で呼ばないといけない。
            // 別スレで呼ぶと関係ないとこで他のモジュール群が死ぬ。
            var currentHeadInObstacle = this._playerHeadAndObstacleInteraction.playerHeadIsInObstacle;

            if (!this._headInObstacle && currentHeadInObstacle) {
                this._headInObstacle = true;
                this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ObstacleEnter);
            }
            else if (this._headInObstacle && !currentHeadInObstacle) {
                this._headInObstacle = false;
                this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ObstacleExit);
            }
        }

        private void UpdateCurrentSongTime()
        {
            var songTime = Mathf.FloorToInt(this._audioTimeSource.songTime);
            if (this._gameStatus.currentSongTime != songTime) {
                this._gameStatus.currentSongTime = songTime;
                this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.BeatmapEvent);
            }
        }

        private void UpdateModMultiplier()
        {
            var energy = this._gameEnergyCounter.energy;

            this._gameStatus.modifierMultiplier = this._gameplayModifiersSO.GetTotalMultiplier(this._gameplayModifiersSO.CreateModifierParamsList(this._gameplayModifiers), energy);

            this._gameStatus.maxScore = this._gameplayModifiersSO.MaxModifiedScoreForMaxMultipliedScore(ScoreModel.ComputeMaxMultipliedScoreForBeatmap(this._beatmapData), this._gameplayModifiersSO.CreateModifierParamsList(this._gameplayModifiers), this._gameplayModifiersSO, energy);
            this._gameStatus.maxRank = RankModelHelper.MaxRankForGameplayModifiers(this._gameplayModifiers, this._gameplayModifiersSO, energy).ToString();
        }

        private void OnGamePause()
        {
            this._gameStatus.paused = Utility.GetCurrentTime();

            this._statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Pause);
        }

        private void OnGameResume()
        {
            this._gameStatus.start = Utility.GetCurrentTime() - (long)(this._audioTimeSource.songTime * 1000f / this._gameStatus.songSpeedMultiplier);
            this._gameStatus.paused = 0;

            this._statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Resume);
        }
        private void OnNoteWasSpawnedEvent(NoteController obj)
        {
            this.SetNoteCutStatus(obj);
            this._statusManager.EmitStatusUpdate(ChangedProperty.NoteCut, BeatSaberEvent.NoteSpawned);
        }
        private void OnNoteWasMissedEvent(NoteController obj)
        {
            // Event order: combo, multiplier, scoreController.noteWasMissed, (LateUpdate) scoreController.scoreDidChange
            var noteData = obj.noteData;
            this._gameStatus.batteryEnergy = this._gameEnergyCounter.batteryEnergy;
            this._gameStatus.energy = this._gameEnergyCounter.energy;

            this.SetNoteCutStatus(obj, default, false);

            if (noteData.colorType == ColorType.None) {
                this._gameStatus.passedBombs++;

                this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.BombMissed);
            }
            else {
                this._gameStatus.passedNotes++;
                this._gameStatus.missedNotes++;

                this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteMissed);
            }
        }

        private void OnNoteWasCutEvent(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            // Event order: combo, multiplier, scoreController.noteWasCut, (LateUpdate) scoreController.scoreDidChange, afterCut, (LateUpdate) scoreController.scoreDidChange
            var noteData = noteController.noteData;
            var multiplier = this._multiplier;
            this.SetNoteCutStatus(noteController, noteCutInfo);
            this._gameStatus.finalScore = -1;
            this._gameStatus.cutMultiplier = multiplier;
            if (noteData.colorType == ColorType.None) {
                this._gameStatus.passedBombs++;
                this._gameStatus.hitBombs++;

                this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.BombCut);
            }
            else {
                this._gameStatus.passedNotes++;

                if (noteCutInfo.allIsOK) {
                    this._gameStatus.hitNotes++;
                    this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteCut);
                }
                else {
                    this._gameStatus.missedNotes++;
                    this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteMissed);
                }
            }
            if (noteCutInfo.allIsOK) {
                this._cutBufferPool.Spawn(noteCutInfo, multiplier, noteController, this);
            }
        }

        private void ScoreController_scoringForNoteStartedEvent(ScoringElement obj)
        {
            if (obj is GoodCutScoringElement element) {
                this._gameStatus.cutDistanceScore = element.cutScoreBuffer.centerDistanceCutScore;
                this._gameStatus.initialScore = element.cutScore;
                this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteCut);
            }
        }

        private void ScoreController_scoringForNoteFinishedEvent(ScoringElement obj)
        {
            if (obj is GoodCutScoringElement element) {
                this._gameStatus.cutDistanceScore = element.cutScoreBuffer.centerDistanceCutScore;
                this._gameStatus.finalScore = element.cutScore;
                this._statusManager.EmitStatusUpdate(ChangedProperty.PerformanceAndNoteCut, BeatSaberEvent.NoteCut);
            }
        }

        private void SetNoteCutStatus(NoteController noteController, in NoteCutInfo noteCutInfo = default, bool initialCut = true)
        {
            var noteData = noteController.noteData;
            var entity = this._notePool.Spawn(noteData, this._gameplayModifiers.noArrows);
            this._gameStatus.ResetNoteCut();
            // Backwards compatibility for <1.12.1
            this._gameStatus.noteID = -1;
            // Check the near notes first for performance
            if (this._noteToIdMapping.TryGetValue(entity, out var noteID)) {
                this._gameStatus.noteID = noteID;
                if (this._lastNoteId < noteID) {
                    this._lastNoteId = noteID;
                }
            }
            else {
                this._gameStatus.noteID = this._lastNoteId;
            }
            // Backwards compatibility for <1.12.1
            var colorName = noteData.colorType.ToString();
            switch (noteData.colorType) {
                case ColorType.ColorA:
                    colorName = "NoteA";
                    break;
                case ColorType.ColorB:
                    colorName = "NoteB";
                    break;
                case ColorType.None:
                    colorName = "Bomb";
                    break;
                default:
                    break;
            }
            this._gameStatus.noteType = colorName;
            this._gameStatus.noteCutDirection = noteData.cutDirection.ToString();
            this._gameStatus.noteLine = noteData.lineIndex;
            this._gameStatus.noteLayer = (int)noteData.noteLineLayer;
            // If long notes are ever introduced, this name will make no sense
            this._gameStatus.timeToNextBasicNote = noteData.timeToNextColorNote;
            if (!EqualityComparer<NoteCutInfo>.Default.Equals(noteCutInfo, default)) {
                var noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(noteCutInfo.noteData.scoringType);
                var rateBeforeCut = noteScoreDefinition.maxBeforeCutScore > 0 && noteScoreDefinition.minBeforeCutScore != noteScoreDefinition.maxBeforeCutScore;
                var rateAfterCut = noteScoreDefinition.maxAfterCutScore > 0 && noteScoreDefinition.minAfterCutScore != noteScoreDefinition.maxAfterCutScore;
                var noteTransform = noteController.noteTransform;
                this._gameStatus.speedOK = noteCutInfo.speedOK;
                this._gameStatus.directionOK = noteCutInfo.directionOK;
                this._gameStatus.saberTypeOK = noteCutInfo.saberTypeOK;
                this._gameStatus.wasCutTooSoon = noteCutInfo.wasCutTooSoon;
                this._gameStatus.saberSpeed = noteCutInfo.saberSpeed;
                var saberDir = noteTransform.InverseTransformDirection(noteCutInfo.saberDir);
                this._gameStatus.saberDirX = saberDir[0];
                this._gameStatus.saberDirY = saberDir[1];
                this._gameStatus.saberDirZ = saberDir[2];
                var rating = noteCutInfo.saberMovementData?.ComputeSwingRating();
                this._gameStatus.swingRating = noteCutInfo.saberMovementData == null ? -1 : rating.Value;
                this._gameStatus.afterSwingRating = rateAfterCut ? 0 : 1;
                this._gameStatus.beforSwingRating = rateAfterCut ? rating.Value : 1;
                this._gameStatus.saberType = noteCutInfo.saberType.ToString();
                this._gameStatus.timeDeviation = noteCutInfo.timeDeviation;
                this._gameStatus.cutDirectionDeviation = noteCutInfo.cutDirDeviation;
                var cutPoint = noteTransform.InverseTransformPoint(noteCutInfo.cutPoint);
                this._gameStatus.cutPointX = cutPoint[0];
                this._gameStatus.cutPointY = cutPoint[1];
                this._gameStatus.cutPointZ = cutPoint[2];
                var cutNormal = noteTransform.InverseTransformDirection(noteCutInfo.cutNormal);
                this._gameStatus.cutNormalX = cutNormal[0];
                this._gameStatus.cutNormalY = cutNormal[1];
                this._gameStatus.cutNormalZ = cutNormal[2];
                this._gameStatus.cutDistanceToCenter = noteCutInfo.cutDistanceToCenter;
            }
            this._notePool.Despawn(entity);
        }

        private void OnScoreDidChange(int scoreBeforeMultiplier, int scoreAfterMultiplier)
        {
            this._gameStatus.rawScore = scoreBeforeMultiplier;
            this._gameStatus.score = scoreAfterMultiplier;
            this._gameStatus.currentMaxScore = this._scoreController.immediateMaxPossibleModifiedScore;
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }
        private void OnComboDidChange(int combo)
        {
            this._gameStatus.combo = combo;
            // public int ScoreController#maxCombo
            this._gameStatus.maxCombo = (this._comboController as ComboController).maxCombo;
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void OnMultiplierDidChange(int multiplier, float multiplierProgress)
        {
            this._multiplier = multiplier;
            this._gameStatus.multiplier = multiplier;
            this._gameStatus.multiplierProgress = multiplierProgress;
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void OnLevelFinished()
        {
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.Finished);
        }

        private void OnLevelFailed()
        {
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.Failed);
        }

        private void OnEnergyDidReach0Event()
        {
            if (this._gameStatus.modNoFail) {
                this._gameStatus.softFailed = true;
                this.UpdateModMultiplier();
                this._statusManager.EmitStatusUpdate(ChangedProperty.BeatmapAndPerformanceAndMod, BeatSaberEvent.SoftFailed);
            }
        }

        private void OnBeatmapEventDidTrigger(BeatmapEventData beatmapEventData)
        {
            this._gameStatus.beatmapEventType = (int)beatmapEventData.type;
            this._gameStatus.beatmapEventValue = beatmapEventData.executionOrder;

            this._statusManager.EmitStatusUpdate(ChangedProperty.BeatmapEvent, BeatSaberEvent.BeatmapEvent);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private IStatusManager _statusManager;
        private GameStatus _gameStatus;
        private CustomCutBuffer.Pool _cutBufferPool;
        private NoteDataEntity.Pool _notePool;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private PauseController _pauseController;
        private IScoreController _scoreController;
        private IComboController _comboController;
        private GameplayModifiers _gameplayModifiers;
        private IAudioTimeSource _audioTimeSource;
        private BeatmapCallbacksController _beatmapObjectCallbackController;
        private PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction;
        private GameEnergyCounter _gameEnergyCounter;
        private MultiplayerLocalActivePlayerFacade _multiplayerLocalActivePlayerFacade;
        private RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRankCounter;
        private BeatmapObjectManager _beatmapObjectManager;
        private ILevelEndActions _levelEndActions;
        private GameplayModifiersModelSO _gameplayModifiersSO;
        private bool _headInObstacle = false;
        private Thread _thread;
        private int _lastNoteId = 0;
        private bool _disposedValue;
        private int _multiplier = 1;
        private BeatmapDataCallbackWrapper _eventDataCallbackWrapper;
        private IReadonlyBeatmapData _beatmapData;
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
            IComboController comboController,
            GameplayModifiers gameplayModifiers,
            IAudioTimeSource audioTimeSource,
            IReadonlyBeatmapData readonlyBeatmapData,
            BeatmapCallbacksController beatmapObjectCallbackController,
            PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction,
            GameEnergyCounter gameEnergyCounter,
            RelativeScoreAndImmediateRankCounter relative,
            BeatmapObjectManager beatmapObjectManager,
            DiContainer diContainer)
        {
            this._statusManager = statusManager;
            this._gameStatus = gameStatus;
            this._cutBufferPool = customCutBufferPool;
            this._notePool = noteEntityPool;
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._scoreController = score;
            this._gameplayModifiers = gameplayModifiers;
            this._audioTimeSource = audioTimeSource;
            this._beatmapObjectCallbackController = beatmapObjectCallbackController;
            this._playerHeadAndObstacleInteraction = playerHeadAndObstacleInteraction;
            this._gameEnergyCounter = gameEnergyCounter;
            this._relativeScoreAndImmediateRankCounter = relative;
            this._beatmapObjectManager = beatmapObjectManager;
            this._comboController = comboController;
            this._beatmapData = readonlyBeatmapData;
            if (this._scoreController is ScoreController scoreController) {
                this._gameplayModifiersSO = scoreController.GetField<GameplayModifiersModelSO, ScoreController>("_gameplayModifiersModel");
            }
            this._pauseController = diContainer.TryResolve<PauseController>();
            this._levelEndActions = diContainer.TryResolve<ILevelEndActions>();
            this._multiplayerLocalActivePlayerFacade = diContainer.TryResolve<MultiplayerLocalActivePlayerFacade>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    Plugin.Logger.Debug("dispose call");
                    try {
                        this._gameStatus.scene = "Menu"; // XXX: impossible because multiplayerController is always cleaned up before this

                        this._gameStatus?.ResetMapInfo();

                        this._gameStatus?.ResetPerformance();

                        // Clear note id mappings.
                        this._noteToIdMapping?.Clear();

                        this._statusManager?.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.Menu);

                        this._thread?.Abort();

                        if (this._pauseController != null) {
                            this._pauseController.didPauseEvent -= this.OnGamePause;
                            this._pauseController.didResumeEvent -= this.OnGameResume;
                        }

                        if (this._scoreController != null) {
                            this._scoreController.scoreDidChangeEvent -= this.OnScoreDidChange;
                            this._scoreController.multiplierDidChangeEvent -= this.OnMultiplierDidChange;
                        }
                        if (this._comboController != null) {
                            this._comboController.comboDidChangeEvent -= this.OnComboDidChange;
                        }

                        if (this._multiplayerLocalActivePlayerFacade != null) {
                            this._multiplayerLocalActivePlayerFacade.playerDidFinishEvent -= this.OnMultiplayerLevelFinished;
                            this._multiplayerLocalActivePlayerFacade = null;
                        }

                        if (this._levelEndActions != null) {
                            this._levelEndActions.levelFinishedEvent -= this.OnLevelFinished;
                            this._levelEndActions.levelFailedEvent -= this.OnLevelFailed;
                        }

                        if (this._beatmapObjectCallbackController != null) {
                            this._beatmapObjectCallbackController.RemoveBeatmapCallback(this._eventDataCallbackWrapper);
                            //this.beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= this.OnBeatmapEventDidTrigger;
                        }

                        if (this._beatmapObjectManager != null) {
                            this._beatmapObjectManager.noteWasSpawnedEvent -= this.OnNoteWasSpawnedEvent;
                            this._beatmapObjectManager.noteWasMissedEvent -= this.OnNoteWasMissedEvent;
                        }

                        if (this._gameEnergyCounter != null) {
                            this._gameEnergyCounter.gameEnergyDidChangeEvent -= this.OnEnergyChanged;
                            this._gameEnergyCounter.gameEnergyDidReach0Event -= this.OnEnergyDidReach0Event;
                        }
                        if (this._relativeScoreAndImmediateRankCounter) {
                            this._relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
                        }
                    }
                    catch (Exception e) {
                        Plugin.Logger.Error(e);
                    }
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            Plugin.Logger.Info("InitializeAsync()");
            // Check for multiplayer early to abort if needed: gameplay controllers don't exist in multiplayer until later
            this._gameStatus.scene = "Song";
            // Register event listeners
            // PauseController doesn't exist in multiplayer
            if (this._pauseController != null) {
                // public event Action PauseController#didPauseEvent;
                this._pauseController.didPauseEvent += this.OnGamePause;
                // public event Action PauseController#didResumeEvent;
                this._pauseController.didResumeEvent += this.OnGameResume;
            }
            // public ScoreController#scoreDidChangeEvent<int, int> // score
            this._scoreController.scoreDidChangeEvent += this.OnScoreDidChange;
            // public ScoreController#comboDidChangeEvent<int> // combo
            this._comboController.comboDidChangeEvent += this.OnComboDidChange;
            // public ScoreController#multiplierDidChangeEvent<int, float> // multiplier, progress [0..1]
            this._scoreController.multiplierDidChangeEvent += this.OnMultiplierDidChange;
            this._scoreController.scoringForNoteStartedEvent += this.ScoreController_scoringForNoteStartedEvent;
            this._scoreController.scoringForNoteFinishedEvent += this.ScoreController_scoringForNoteFinishedEvent;
            // public event Action<BeatmapEventData> BeatmapObjectCallbackController#beatmapEventDidTriggerEvent
            //this.beatmapObjectCallbackController.beatmapEventDidTriggerEvent += this.OnBeatmapEventDidTrigger;
            this._eventDataCallbackWrapper = this._beatmapObjectCallbackController.AddBeatmapCallback(0, new BeatmapDataCallback<BasicBeatmapEventData>(this.OnBeatmapEventDidTrigger));
            this._beatmapObjectManager.noteWasSpawnedEvent += this.OnNoteWasSpawnedEvent;
            this._beatmapObjectManager.noteWasMissedEvent += this.OnNoteWasMissedEvent;
            this._relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
            // public event Action GameEnergyCounter#gameEnergyDidReach0Event;
            this._gameEnergyCounter.gameEnergyDidReach0Event += this.OnEnergyDidReach0Event;
            this._gameEnergyCounter.gameEnergyDidChangeEvent += this.OnEnergyChanged;

            if (this._multiplayerLocalActivePlayerFacade != null) {
                this._multiplayerLocalActivePlayerFacade.playerDidFinishEvent += this.OnMultiplayerLevelFinished;
            }
            if (this._levelEndActions != null) {
                this._levelEndActions.levelFinishedEvent += this.OnLevelFinished;
                this._levelEndActions.levelFailedEvent += this.OnLevelFailed;
            }
            var diff = this._gameplayCoreSceneSetupData.difficultyBeatmap;
            var level = diff.level;

            this._gameplayModifiers = this._gameplayCoreSceneSetupData.gameplayModifiers;
            var playerSettings = this._gameplayCoreSceneSetupData.playerSpecificSettings;
            var practiceSettings = this._gameplayCoreSceneSetupData.practiceSettings;

            var songSpeedMul = this._gameplayModifiers.songSpeedMul;
            if (practiceSettings != null) {
                songSpeedMul = practiceSettings.songSpeedMul;
            }
            // Generate NoteData to id mappings for backwards compatiblity with <1.12.1
            this._noteToIdMapping.Clear();

            this._lastNoteId = 0;
            var beatmapData = await diff.GetBeatmapDataBasicInfoAsync().ConfigureAwait(true);
            foreach (var note in this._beatmapData.allBeatmapDataItems.OfType<NoteData>().OrderBy(x => x.time).ThenBy(x => x.lineIndex).ThenBy(x => x.noteLineLayer).ThenBy(x => x.cutDirection).Select((x, i) => new { note = x, index = i })) {
                this._noteToIdMapping.TryAdd(new NoteDataEntity(note.note, this._gameplayModifiers.noArrows), note.index);
            }
            this._gameStatus.songName = level.songName;
            this._gameStatus.songSubName = level.songSubName;
            this._gameStatus.songAuthorName = level.songAuthorName;
            this._gameStatus.levelAuthorName = level.levelAuthorName;
            this._gameStatus.songBPM = level.beatsPerMinute;
            this._gameStatus.noteJumpSpeed = diff.noteJumpMovementSpeed;
            this._gameStatus.noteJumpStartBeatOffset = diff.noteJumpStartBeatOffset;
            // 13 is "custom_level_" and 40 is the magic number for the length of the SHA-1 hash
            this._gameStatus.songHash = Regex.IsMatch(level.levelID, "^custom_level_[0-9A-F]{40}", RegexOptions.IgnoreCase) && !level.levelID.EndsWith(" WIP") ? level.levelID.Substring(13, 40) : null;
            this._gameStatus.levelId = level.levelID;
            this._gameStatus.songTimeOffset = (long)(level.songTimeOffset * 1000f / songSpeedMul);
            this._gameStatus.length = (long)(level.beatmapLevelData.audioClip.length * 1000f / songSpeedMul);
            this._gameStatus.start = Utility.GetCurrentTime() - (long)(this._audioTimeSource.songTime * 1000f / songSpeedMul);
            if (practiceSettings != null) {
                this._gameStatus.start -= (long)(practiceSettings.startSongTime * 1000f / songSpeedMul);
            }

            this._gameStatus.paused = 0;
            this._gameStatus.difficulty = diff.difficulty.Name();
            this._gameStatus.difficultyEnum = Enum.GetName(typeof(BeatmapDifficulty), diff.difficulty);
            this._gameStatus.characteristic = diff.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            this._gameStatus.notesCount = beatmapData.cuttableNotesCount;
            this._gameStatus.bombsCount = beatmapData.bombsCount;
            this._gameStatus.obstaclesCount = beatmapData.obstaclesCount;
            this._gameStatus.environmentName = level.environmentInfo.sceneInfo.sceneName;
            var colorScheme = this._gameplayCoreSceneSetupData.colorScheme ?? new ColorScheme(this._gameplayCoreSceneSetupData.environmentInfo.colorScheme);
            this._gameStatus.colorSaberA = colorScheme.saberAColor;
            this._gameStatus.colorSaberB = colorScheme.saberBColor;
            this._gameStatus.colorEnvironment0 = colorScheme.environmentColor0;
            this._gameStatus.colorEnvironment1 = colorScheme.environmentColor1;
            if (colorScheme.supportsEnvironmentColorBoost) {
                this._gameStatus.colorEnvironmentBoost0 = colorScheme.environmentColor0Boost;
                this._gameStatus.colorEnvironmentBoost1 = colorScheme.environmentColor1Boost;
            }
            this._gameStatus.colorObstacle = colorScheme.obstaclesColor;
            try {
                // From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
                // Modified to correctly handle texture atlases. Fixes #82.
                var active = RenderTexture.active;

                var sprite = await level.GetCoverImageAsync(CancellationToken.None);
                var texture = sprite.texture;
                var temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

                Graphics.Blit(texture, temporary);
                RenderTexture.active = temporary;

                var spriteRect = sprite.rect;
                var uv = sprite.uv[0];

                var cover = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
                // Unity sucks. The coordinates of the sprite on its texture atlas are only accessible through the Sprite.uv property since rect always returns `x=0,y=0`, so we need to convert them back into texture space.
                cover.ReadPixels(new Rect(
                    uv.x * texture.width,
                    texture.height - uv.y * texture.height,
                    spriteRect.width,
                    spriteRect.height
                ), 0, 0);
                cover.Apply();

                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(temporary);

                this._gameStatus.songCover = Convert.ToBase64String(ImageConversion.EncodeToPNG(cover));
            }
            catch (Exception e) {
                Plugin.Logger.Error(e);
                this._gameStatus.songCover = null;
            }

            this.UpdateModMultiplier();
            this._gameStatus.songSpeedMultiplier = songSpeedMul;
            this._gameStatus.batteryLives = this._gameEnergyCounter.batteryLives;

            this._gameStatus.modObstacles = this._gameplayModifiers.enabledObstacleType.ToString();
            this._gameStatus.modInstaFail = this._gameplayModifiers.instaFail;
            this._gameStatus.modNoFail = this._gameplayModifiers.noFailOn0Energy;
            this._gameStatus.modBatteryEnergy = this._gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery;
            this._gameStatus.modDisappearingArrows = this._gameplayModifiers.disappearingArrows;
            this._gameStatus.modNoBombs = this._gameplayModifiers.noBombs;
            this._gameStatus.modSongSpeed = this._gameplayModifiers.songSpeed.ToString();
            this._gameStatus.modNoArrows = this._gameplayModifiers.noArrows;
            this._gameStatus.modGhostNotes = this._gameplayModifiers.ghostNotes;
            this._gameStatus.modFailOnSaberClash = this._gameplayModifiers.failOnSaberClash;
            this._gameStatus.modStrictAngles = this._gameplayModifiers.strictAngles;
            this._gameStatus.modFastNotes = this._gameplayModifiers.fastNotes;
            this._gameStatus.modSmallNotes = this._gameplayModifiers.smallCubes;
            this._gameStatus.modProMode = this._gameplayModifiers.proMode;
            this._gameStatus.modZenMode = this._gameplayModifiers.zenMode;

            this._gameStatus.staticLights = (diff.difficulty == BeatmapDifficulty.ExpertPlus ? playerSettings.environmentEffectsFilterExpertPlusPreset : playerSettings.environmentEffectsFilterDefaultPreset) != EnvironmentEffectsFilterPreset.AllEffects;
            this._gameStatus.leftHanded = playerSettings.leftHanded;
            this._gameStatus.playerHeight = playerSettings.playerHeight;
            this._gameStatus.sfxVolume = playerSettings.sfxVolume;
            this._gameStatus.reduceDebris = playerSettings.reduceDebris;
            this._gameStatus.noHUD = playerSettings.noTextsAndHuds;
            this._gameStatus.advancedHUD = playerSettings.advancedHud;
            this._gameStatus.autoRestart = playerSettings.autoRestart;
            this._gameStatus.saberTrailIntensity = playerSettings.saberTrailIntensity;
            this._gameStatus.environmentEffects = (diff.difficulty == BeatmapDifficulty.ExpertPlus ? playerSettings.environmentEffectsFilterExpertPlusPreset : playerSettings.environmentEffectsFilterDefaultPreset).ToString();
            this._gameStatus.hideNoteSpawningEffect = playerSettings.hideNoteSpawnEffect;

            this._thread = new Thread(new ThreadStart(() =>
            {
                while (!this._disposedValue) {
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
            this._statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
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
