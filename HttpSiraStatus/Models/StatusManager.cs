using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Zenject;

namespace HttpSiraStatus
{
    public class StatusManager : IStatusManager, IDisposable
    {
        [Inject]
        public GameStatus GameStatus { get; }
        public JSONObject StatusJSON { get; } = new JSONObject();
        public JSONObject NoteCutJSON { get; } = new JSONObject();
        public JSONObject BeatmapEventJSON { get; } = new JSONObject();
        public ConcurrentQueue<JSONObject> JsonQueue { get; } = new ConcurrentQueue<JSONObject>();

        [Inject]
        private MemoryPool<JSONObject> memoryPool;
        private MemoryPoolContainer<JSONObject> memoryPoolContainer;

        public event SendEventHandler SendEvent;

        private Thread thread;
        private bool disposedValue;

        [Inject]
        void Constractor()
        {
            this.memoryPoolContainer = new MemoryPoolContainer<JSONObject>(this.memoryPool);

            UpdateAll();
            this.thread = new Thread(new ThreadStart(this.RaiseSendEvent));
            this.thread.Start();
        }

        public void EmitStatusUpdate(ChangedProperty changedProps, BeatSaberEvent e)
        {
            GameStatus.updateCause = e.GetDescription();
            if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game) UpdateGameJSON();
            if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap) UpdateBeatmapJSON();
            if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance) UpdatePerformanceJSON();
            if ((changedProps & ChangedProperty.NoteCut) == ChangedProperty.NoteCut) UpdateNoteCutJSON();
            if ((changedProps & ChangedProperty.Mod) == ChangedProperty.Mod) {
                UpdateModJSON();
                UpdatePlayerSettingsJSON();
            }
            if ((changedProps & ChangedProperty.BeatmapEvent) == ChangedProperty.BeatmapEvent) UpdateBeatmapEventJSON();

            this.EnqueueMessage(changedProps, e);
        }

        private void EnqueueMessage(ChangedProperty changedProps, BeatSaberEvent e)
        {
            var eventJSON = this.memoryPoolContainer.Spawn();
            eventJSON["event"] = e.GetDescription();

            if ((changedProps & (ChangedProperty.Game | ChangedProperty.Beatmap | ChangedProperty.Performance | ChangedProperty.Mod))
                == (ChangedProperty.Game | ChangedProperty.Beatmap | ChangedProperty.Performance | ChangedProperty.Mod)) {
                eventJSON["status"] = this.StatusJSON.Clone();
            }
            else {
                var status = new JSONObject();

                if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game) status["game"] = this.StatusJSON["game"].Clone();
                if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap) status["beatmap"] = this.StatusJSON["beatmap"].Clone();
                if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance) status["performance"] = this.StatusJSON["performance"].Clone();
                if ((changedProps & ChangedProperty.Mod) == ChangedProperty.Mod) {
                    status["mod"] = this.StatusJSON["mod"].Clone();
                    status["playerSettings"] = this.StatusJSON["playerSettings"].Clone();
                }
                eventJSON["status"] = status;
            }
            if ((changedProps & ChangedProperty.NoteCut) == ChangedProperty.NoteCut) {
                eventJSON["noteCut"] = this.NoteCutJSON.Clone();
            }
            if ((changedProps & ChangedProperty.BeatmapEvent) == ChangedProperty.BeatmapEvent) {
                eventJSON["beatmapEvent"] = this.BeatmapEventJSON.Clone();
            }
            this.JsonQueue.Enqueue(eventJSON);
        }

        private void RaiseSendEvent()
        {
            while (true) {
                try {
                    while (this.JsonQueue.TryDequeue(out var json)) {
                        this.SendEvent?.Invoke(this, new SendEventArgs(json.Clone()));
                        this.memoryPoolContainer.Despawn(json);
                    }
                }
                catch (Exception e) {
                    Plugin.Logger.Error(e);
                }
                Thread.Sleep(1);
            }
        }

        private void UpdateAll()
        {
            try {
                UpdateGameJSON();
                UpdateBeatmapJSON();
                UpdatePerformanceJSON();
                UpdateNoteCutJSON();
                UpdateModJSON();
                UpdatePlayerSettingsJSON();
                UpdateBeatmapEventJSON();
            }
            catch (Exception e) {
                Plugin.Logger.Error(e);
            }
        }

        private void UpdateGameJSON()
        {
            if (StatusJSON["game"] == null) StatusJSON["game"] = new JSONObject();
            var gameJSON = StatusJSON["game"].AsObject;

            gameJSON["pluginVersion"] = HttpSiraStatus.Plugin.PluginVersion;
            gameJSON["gameVersion"] = HttpSiraStatus.Plugin.GameVersion;
            gameJSON["scene"] = StringOrNull(GameStatus.scene);
            gameJSON["mode"] = StringOrNull(GameStatus.mode == null ? null : (GameStatus.multiplayer ? "Multiplayer" : GameStatus.partyMode ? "Party" : "Solo") + GameStatus.mode);
        }

        private void UpdateBeatmapJSON()
        {
            if (GameStatus.songName == null) {
                StatusJSON["beatmap"] = null;
                return;
            }

            if (StatusJSON["beatmap"] == null) StatusJSON["beatmap"] = new JSONObject();
            var beatmapJSON = StatusJSON["beatmap"].AsObject;

            beatmapJSON["songName"] = StringOrNull(GameStatus.songName);
            beatmapJSON["songSubName"] = StringOrNull(GameStatus.songSubName);
            beatmapJSON["songAuthorName"] = StringOrNull(GameStatus.songAuthorName);
            beatmapJSON["levelAuthorName"] = StringOrNull(GameStatus.levelAuthorName);
            beatmapJSON["songCover"] = String.IsNullOrEmpty(GameStatus.songCover) ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONString(GameStatus.songCover);
            beatmapJSON["songHash"] = StringOrNull(GameStatus.songHash);
            beatmapJSON["levelId"] = StringOrNull(GameStatus.levelId);
            beatmapJSON["songBPM"] = GameStatus.songBPM;
            beatmapJSON["noteJumpSpeed"] = GameStatus.noteJumpSpeed;
            beatmapJSON["songTimeOffset"] = new JSONNumber(GameStatus.songTimeOffset);
            beatmapJSON["start"] = GameStatus.start == 0 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(GameStatus.start);
            beatmapJSON["paused"] = GameStatus.paused == 0 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(GameStatus.paused);
            beatmapJSON["length"] = new JSONNumber(GameStatus.length);
            beatmapJSON["difficulty"] = StringOrNull(GameStatus.difficulty);
            beatmapJSON["notesCount"] = GameStatus.notesCount;
            beatmapJSON["bombsCount"] = GameStatus.bombsCount;
            beatmapJSON["obstaclesCount"] = GameStatus.obstaclesCount;
            beatmapJSON["maxScore"] = GameStatus.maxScore;
            beatmapJSON["maxRank"] = GameStatus.maxRank;
            beatmapJSON["environmentName"] = GameStatus.environmentName;
        }

        private void UpdatePerformanceJSON()
        {
            if (GameStatus.start == 0) {
                StatusJSON["performance"] = null;
                return;
            }

            if (StatusJSON["performance"] == null) StatusJSON["performance"] = new JSONObject();
            var performanceJSON = StatusJSON["performance"].AsObject;

            performanceJSON["rawScore"] = GameStatus.rawScore;
            performanceJSON["score"] = GameStatus.score;
            performanceJSON["currentMaxScore"] = GameStatus.currentMaxScore;
            performanceJSON["rank"] = GameStatus.rank;
            performanceJSON["passedNotes"] = GameStatus.passedNotes;
            performanceJSON["hitNotes"] = GameStatus.hitNotes;
            performanceJSON["missedNotes"] = GameStatus.missedNotes;
            performanceJSON["lastNoteScore"] = GameStatus.lastNoteScore;
            performanceJSON["passedBombs"] = GameStatus.passedBombs;
            performanceJSON["hitBombs"] = GameStatus.hitBombs;
            performanceJSON["combo"] = GameStatus.combo;
            performanceJSON["maxCombo"] = GameStatus.maxCombo;
            performanceJSON["multiplier"] = GameStatus.multiplier;
            performanceJSON["multiplierProgress"] = GameStatus.multiplierProgress;
            performanceJSON["batteryEnergy"] = GameStatus.modBatteryEnergy || GameStatus.modInstaFail ? (JSONNode)new JSONNumber(GameStatus.batteryEnergy) : (JSONNode)JSONNull.CreateOrGet();
            performanceJSON["energy"] = new JSONNumber(this.GameStatus.energy);
            performanceJSON["softFailed"] = GameStatus.softFailed;
        }

        private void UpdateNoteCutJSON()
        {
            NoteCutJSON["noteID"] = GameStatus.noteID;
            NoteCutJSON["noteType"] = StringOrNull(GameStatus.noteType);
            NoteCutJSON["noteCutDirection"] = StringOrNull(GameStatus.noteCutDirection);
            NoteCutJSON["noteLine"] = GameStatus.noteLine;
            NoteCutJSON["noteLayer"] = GameStatus.noteLayer;
            NoteCutJSON["speedOK"] = GameStatus.speedOK;
            NoteCutJSON["directionOK"] = GameStatus.noteType == "Bomb" ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONBool(GameStatus.directionOK);
            NoteCutJSON["saberTypeOK"] = GameStatus.noteType == "Bomb" ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONBool(GameStatus.saberTypeOK);
            NoteCutJSON["wasCutTooSoon"] = GameStatus.wasCutTooSoon;
            NoteCutJSON["initialScore"] = GameStatus.initialScore == -1 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(GameStatus.initialScore);
            NoteCutJSON["finalScore"] = GameStatus.finalScore == -1 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(GameStatus.finalScore);
            NoteCutJSON["cutDistanceScore"] = GameStatus.cutDistanceScore == -1 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(GameStatus.cutDistanceScore);
            NoteCutJSON["swingRating"] = GameStatus.swingRating;
            NoteCutJSON["multiplier"] = GameStatus.cutMultiplier;
            NoteCutJSON["saberSpeed"] = GameStatus.saberSpeed;
            if (!NoteCutJSON["saberDir"].IsArray) NoteCutJSON["saberDir"] = new JSONArray();
            NoteCutJSON["saberDir"][0] = GameStatus.saberDirX;
            NoteCutJSON["saberDir"][1] = GameStatus.saberDirY;
            NoteCutJSON["saberDir"][2] = GameStatus.saberDirZ;
            NoteCutJSON["saberType"] = StringOrNull(GameStatus.saberType);
            NoteCutJSON["timeDeviation"] = GameStatus.timeDeviation;
            NoteCutJSON["cutDirectionDeviation"] = GameStatus.cutDirectionDeviation;
            if (!NoteCutJSON["cutPoint"].IsArray) NoteCutJSON["cutPoint"] = new JSONArray();
            NoteCutJSON["cutPoint"][0] = GameStatus.cutPointX;
            NoteCutJSON["cutPoint"][1] = GameStatus.cutPointY;
            NoteCutJSON["cutPoint"][2] = GameStatus.cutPointZ;
            if (!NoteCutJSON["cutNormal"].IsArray) NoteCutJSON["cutNormal"] = new JSONArray();
            NoteCutJSON["cutNormal"][0] = GameStatus.cutNormalX;
            NoteCutJSON["cutNormal"][1] = GameStatus.cutNormalY;
            NoteCutJSON["cutNormal"][2] = GameStatus.cutNormalZ;
            NoteCutJSON["cutDistanceToCenter"] = GameStatus.cutDistanceToCenter;
            NoteCutJSON["timeToNextBasicNote"] = GameStatus.timeToNextBasicNote;
        }

        private void UpdateModJSON()
        {
            if (StatusJSON["mod"] == null) StatusJSON["mod"] = new JSONObject();
            var modJSON = StatusJSON["mod"].AsObject;

            modJSON["multiplier"] = GameStatus.modifierMultiplier;
            modJSON["obstacles"] = GameStatus.modObstacles == null || GameStatus.modObstacles == "NoObstacles" ? (JSONNode)new JSONBool(false) : (JSONNode)new JSONString(GameStatus.modObstacles);
            modJSON["instaFail"] = GameStatus.modInstaFail;
            modJSON["noFail"] = GameStatus.modNoFail;
            modJSON["batteryEnergy"] = GameStatus.modBatteryEnergy;
            modJSON["batteryLives"] = GameStatus.modBatteryEnergy || GameStatus.modInstaFail ? (JSONNode)new JSONNumber(GameStatus.batteryLives) : (JSONNode)JSONNull.CreateOrGet();
            modJSON["disappearingArrows"] = GameStatus.modDisappearingArrows;
            modJSON["noBombs"] = GameStatus.modNoBombs;
            modJSON["songSpeed"] = GameStatus.modSongSpeed;
            modJSON["songSpeedMultiplier"] = GameStatus.songSpeedMultiplier;
            modJSON["noArrows"] = GameStatus.modNoArrows;
            modJSON["ghostNotes"] = GameStatus.modGhostNotes;
            modJSON["failOnSaberClash"] = GameStatus.modFailOnSaberClash;
            modJSON["strictAngles"] = GameStatus.modStrictAngles;
            modJSON["fastNotes"] = GameStatus.modFastNotes;
        }

        private void UpdatePlayerSettingsJSON()
        {
            if (StatusJSON["playerSettings"] == null) StatusJSON["playerSettings"] = new JSONObject();
            var playerSettingsJSON = StatusJSON["playerSettings"].AsObject;

            playerSettingsJSON["staticLights"] = GameStatus.staticLights;
            playerSettingsJSON["leftHanded"] = GameStatus.leftHanded;
            playerSettingsJSON["playerHeight"] = GameStatus.playerHeight;
            playerSettingsJSON["sfxVolume"] = GameStatus.sfxVolume;
            playerSettingsJSON["reduceDebris"] = GameStatus.reduceDebris;
            playerSettingsJSON["noHUD"] = GameStatus.noHUD;
            playerSettingsJSON["advancedHUD"] = GameStatus.advancedHUD;
            playerSettingsJSON["autoRestart"] = GameStatus.autoRestart;
        }

        private void UpdateBeatmapEventJSON()
        {
            BeatmapEventJSON["type"] = GameStatus.beatmapEventType;
            BeatmapEventJSON["value"] = GameStatus.beatmapEventValue;
        }

        private JSONNode StringOrNull(string str)
        {
            return str == null ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONString(str);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this.thread?.Abort();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~StatusManager()
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
    }
}
