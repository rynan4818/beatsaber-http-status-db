using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Models;
using HttpSiraStatus.Util;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using Zenject;

namespace HttpSiraStatus
{
    public class StatusManager : IStatusManager, IDisposable
    {
        public StatusManager()
        {
            this.JsonPool = new ObjectMemoryPool<JSONObject>(null, r => { r.Clear(); }, 20);
        }

        [Inject]
        public GameStatus GameStatus { get; }
        public JSONObject StatusJSON { get; } = new JSONObject();
        public JSONObject NoteCutJSON { get; } = new JSONObject();
        public JSONObject BeatmapEventJSON { get; } = new JSONObject();
        public ObjectMemoryPool<JSONObject> JsonPool { get; }

        public ConcurrentQueue<JSONObject> JsonQueue { get; } = new ConcurrentQueue<JSONObject>();
        public event SendEventHandler SendEvent;
        private Thread thread;
        private bool disposedValue;

        [Inject]
#pragma warning disable IDE0051 // 使用されていないプライベート メンバーを削除する
        private void Constractor()
        {
            this.UpdateAll();
            this.thread = new Thread(new ThreadStart(this.RaiseSendEvent));
            this.thread.Start();
        }
#pragma warning restore IDE0051 // 使用されていないプライベート メンバーを削除する
        public void EmitStatusUpdate(ChangedProperty changedProps, BeatSaberEvent e)
        {
            this.GameStatus.updateCause = e.GetDescription();
            if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game)
                this.UpdateGameJSON();
            if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap)
                this.UpdateBeatmapJSON();
            if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance)
                this.UpdatePerformanceJSON();
            if ((changedProps & ChangedProperty.NoteCut) == ChangedProperty.NoteCut)
                this.UpdateNoteCutJSON();
            if ((changedProps & ChangedProperty.Mod) == ChangedProperty.Mod) {
                this.UpdateModJSON();
                this.UpdatePlayerSettingsJSON();
            }
            if ((changedProps & ChangedProperty.BeatmapEvent) == ChangedProperty.BeatmapEvent) {
                this.UpdateBeatmapEventJSON();
            }
            this.EnqueueMessage(changedProps, e);
        }

        private void EnqueueMessage(ChangedProperty changedProps, BeatSaberEvent e)
        {
            var eventJSON = this.JsonPool.Spawn();
            eventJSON["event"] = e.GetDescription();

            if ((changedProps & ChangedProperty.AllButNoteCut) == ChangedProperty.AllButNoteCut) {
                eventJSON["status"] = this.StatusJSON;
            }
            else {
                var status = new JSONObject();

                if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game)
                    status["game"] = this.StatusJSON["game"];
                if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap)
                    status["beatmap"] = this.StatusJSON["beatmap"];
                if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance)
                    status["performance"] = this.StatusJSON["performance"];
                if ((changedProps & ChangedProperty.Mod) == ChangedProperty.Mod) {
                    status["mod"] = this.StatusJSON["mod"];
                    status["playerSettings"] = this.StatusJSON["playerSettings"];
                }
                eventJSON["status"] = status;
            }
            if ((changedProps & ChangedProperty.NoteCut) == ChangedProperty.NoteCut) {
                eventJSON["noteCut"] = this.NoteCutJSON;
            }
            if ((changedProps & ChangedProperty.BeatmapEvent) == ChangedProperty.BeatmapEvent) {
                eventJSON["beatmapEvent"] = this.BeatmapEventJSON;
            }
            this.JsonQueue.Enqueue(eventJSON);
        }

        private void RaiseSendEvent()
        {
            while (true) {
                try {
                    while (this.JsonQueue.TryDequeue(out var json)) {
                        this.SendEvent?.Invoke(this, new SendEventArgs(json));
                        this.JsonPool.Despawn(json);
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
                this.UpdateGameJSON();
                this.UpdateBeatmapJSON();
                this.UpdatePerformanceJSON();
                this.UpdateNoteCutJSON();
                this.UpdateModJSON();
                this.UpdatePlayerSettingsJSON();
                this.UpdateBeatmapEventJSON();
            }
            catch (Exception e) {
                Plugin.Logger.Error(e);
            }
        }

        private void UpdateGameJSON()
        {
            if (this.StatusJSON["game"] == null)
                this.StatusJSON["game"] = new JSONObject();
            var gameJSON = this.StatusJSON["game"].AsObject;

            gameJSON["pluginVersion"] = Plugin.PluginVersion;
            gameJSON["gameVersion"] = Plugin.GameVersion;
            gameJSON["scene"] = this.StringOrNull(this.GameStatus.scene);
            gameJSON["mode"] = this.StringOrNull(this.GameStatus.mode == null ? null : (this.GameStatus.multiplayer ? "Multiplayer" : this.GameStatus.partyMode ? "Party" : "Solo") + this.GameStatus.mode);
        }

        private void UpdateBeatmapJSON()
        {
            if (this.GameStatus.songName == null) {
                this.StatusJSON["beatmap"] = null;
                return;
            }

            if (this.StatusJSON["beatmap"] == null)
                this.StatusJSON["beatmap"] = new JSONObject();
            var beatmapJSON = this.StatusJSON["beatmap"].AsObject;

            beatmapJSON["songName"] = this.StringOrNull(this.GameStatus.songName);
            beatmapJSON["songSubName"] = this.StringOrNull(this.GameStatus.songSubName);
            beatmapJSON["songAuthorName"] = this.StringOrNull(this.GameStatus.songAuthorName);
            beatmapJSON["levelAuthorName"] = this.StringOrNull(this.GameStatus.levelAuthorName);
            beatmapJSON["songCover"] = String.IsNullOrEmpty(this.GameStatus.songCover) ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONString(this.GameStatus.songCover);
            beatmapJSON["songHash"] = this.StringOrNull(this.GameStatus.songHash);
            beatmapJSON["levelId"] = this.StringOrNull(this.GameStatus.levelId);
            beatmapJSON["songBPM"] = this.GameStatus.songBPM;
            beatmapJSON["noteJumpSpeed"] = this.GameStatus.noteJumpSpeed;
            beatmapJSON["songTimeOffset"] = new JSONNumber(this.GameStatus.songTimeOffset);
            beatmapJSON["start"] = this.GameStatus.start == 0 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(this.GameStatus.start);
            beatmapJSON["paused"] = this.GameStatus.paused == 0 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(this.GameStatus.paused);
            beatmapJSON["length"] = new JSONNumber(this.GameStatus.length);
            beatmapJSON["difficulty"] = this.StringOrNull(this.GameStatus.difficulty);
            beatmapJSON["difficultyEnum"] = this.StringOrNull(this.GameStatus.difficultyEnum);
            beatmapJSON["notesCount"] = this.GameStatus.notesCount;
            beatmapJSON["bombsCount"] = this.GameStatus.bombsCount;
            beatmapJSON["obstaclesCount"] = this.GameStatus.obstaclesCount;
            beatmapJSON["maxScore"] = this.GameStatus.maxScore;
            beatmapJSON["maxRank"] = this.GameStatus.maxRank;
            beatmapJSON["environmentName"] = this.GameStatus.environmentName;

            if (beatmapJSON["color"] == null) {
                beatmapJSON["color"] = new JSONObject();
            }   
            var colorJSON = beatmapJSON["color"].AsObject;

            UpdateColor(GameStatus.colorSaberA, colorJSON, "saberA");
            UpdateColor(GameStatus.colorSaberB, colorJSON, "saberB");
            UpdateColor(GameStatus.colorEnvironment0, colorJSON, "environment0");
            UpdateColor(GameStatus.colorEnvironment1, colorJSON, "environment1");
            UpdateColor(GameStatus.colorEnvironmentBoost0, colorJSON, "environment0Boost");
            UpdateColor(GameStatus.colorEnvironmentBoost1, colorJSON, "environment1Boost");
            UpdateColor(GameStatus.colorObstacle, colorJSON, "obstacle");
        }

        private void UpdateColor(Color? color, JSONObject parent, String key)
        {
            if (color == null) {
                parent[key] = JSONNull.CreateOrGet();
                return;
            }

            var arr = parent[key] as JSONArray ?? new JSONArray();

            arr[0] = Mathf.RoundToInt(((Color)color).r * 255);
            arr[1] = Mathf.RoundToInt(((Color)color).g * 255);
            arr[2] = Mathf.RoundToInt(((Color)color).b * 255);

            parent[key] = arr;
        }

        private void UpdatePerformanceJSON()
        {
            if (this.GameStatus.start == 0) {
                this.StatusJSON["performance"] = null;
                return;
            }

            if (this.StatusJSON["performance"] == null)
                this.StatusJSON["performance"] = new JSONObject();
            var performanceJSON = this.StatusJSON["performance"].AsObject;

            performanceJSON["rawScore"] = this.GameStatus.rawScore;
            performanceJSON["score"] = this.GameStatus.score;
            performanceJSON["currentMaxScore"] = this.GameStatus.currentMaxScore;
            performanceJSON["relativeScore"] = new JSONNumber(this.GameStatus.relativeScore);
            performanceJSON["rank"] = this.GameStatus.rank;
            performanceJSON["passedNotes"] = this.GameStatus.passedNotes;
            performanceJSON["hitNotes"] = this.GameStatus.hitNotes;
            performanceJSON["missedNotes"] = this.GameStatus.missedNotes;
            performanceJSON["lastNoteScore"] = this.GameStatus.lastNoteScore;
            performanceJSON["passedBombs"] = this.GameStatus.passedBombs;
            performanceJSON["hitBombs"] = this.GameStatus.hitBombs;
            performanceJSON["combo"] = this.GameStatus.combo;
            performanceJSON["maxCombo"] = this.GameStatus.maxCombo;
            performanceJSON["multiplier"] = this.GameStatus.multiplier;
            performanceJSON["multiplierProgress"] = this.GameStatus.multiplierProgress;
            performanceJSON["batteryEnergy"] = this.GameStatus.modBatteryEnergy || this.GameStatus.modInstaFail ? (JSONNode)new JSONNumber(this.GameStatus.batteryEnergy) : (JSONNode)JSONNull.CreateOrGet();
            performanceJSON["energy"] = new JSONNumber(this.GameStatus.energy);
            performanceJSON["softFailed"] = this.GameStatus.softFailed;
            performanceJSON["currentSongTime"] = this.GameStatus.currentSongTime;
        }

        private void UpdateNoteCutJSON()
        {
            this.NoteCutJSON["noteID"] = this.GameStatus.noteID;
            this.NoteCutJSON["noteType"] = this.StringOrNull(this.GameStatus.noteType);
            this.NoteCutJSON["noteCutDirection"] = this.StringOrNull(this.GameStatus.noteCutDirection);
            this.NoteCutJSON["noteLine"] = this.GameStatus.noteLine;
            this.NoteCutJSON["noteLayer"] = this.GameStatus.noteLayer;
            this.NoteCutJSON["speedOK"] = this.GameStatus.speedOK;
            this.NoteCutJSON["directionOK"] = this.GameStatus.noteType == "Bomb" ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONBool(this.GameStatus.directionOK);
            this.NoteCutJSON["saberTypeOK"] = this.GameStatus.noteType == "Bomb" ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONBool(this.GameStatus.saberTypeOK);
            this.NoteCutJSON["wasCutTooSoon"] = this.GameStatus.wasCutTooSoon;
            this.NoteCutJSON["initialScore"] = this.GameStatus.initialScore == -1 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(this.GameStatus.initialScore);
            this.NoteCutJSON["finalScore"] = this.GameStatus.finalScore == -1 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(this.GameStatus.finalScore);
            this.NoteCutJSON["cutDistanceScore"] = this.GameStatus.cutDistanceScore == -1 ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONNumber(this.GameStatus.cutDistanceScore);
            this.NoteCutJSON["swingRating"] = this.GameStatus.swingRating;
            this.NoteCutJSON["multiplier"] = this.GameStatus.cutMultiplier;
            this.NoteCutJSON["saberSpeed"] = this.GameStatus.saberSpeed;
            if (!this.NoteCutJSON["saberDir"].IsArray)
                this.NoteCutJSON["saberDir"] = new JSONArray();
            this.NoteCutJSON["saberDir"][0] = this.GameStatus.saberDirX;
            this.NoteCutJSON["saberDir"][1] = this.GameStatus.saberDirY;
            this.NoteCutJSON["saberDir"][2] = this.GameStatus.saberDirZ;
            this.NoteCutJSON["saberType"] = this.StringOrNull(this.GameStatus.saberType);
            this.NoteCutJSON["timeDeviation"] = this.GameStatus.timeDeviation;
            this.NoteCutJSON["cutDirectionDeviation"] = this.GameStatus.cutDirectionDeviation;
            if (!this.NoteCutJSON["cutPoint"].IsArray)
                this.NoteCutJSON["cutPoint"] = new JSONArray();
            this.NoteCutJSON["cutPoint"][0] = this.GameStatus.cutPointX;
            this.NoteCutJSON["cutPoint"][1] = this.GameStatus.cutPointY;
            this.NoteCutJSON["cutPoint"][2] = this.GameStatus.cutPointZ;
            if (!this.NoteCutJSON["cutNormal"].IsArray)
                this.NoteCutJSON["cutNormal"] = new JSONArray();
            this.NoteCutJSON["cutNormal"][0] = this.GameStatus.cutNormalX;
            this.NoteCutJSON["cutNormal"][1] = this.GameStatus.cutNormalY;
            this.NoteCutJSON["cutNormal"][2] = this.GameStatus.cutNormalZ;
            this.NoteCutJSON["cutDistanceToCenter"] = this.GameStatus.cutDistanceToCenter;
            this.NoteCutJSON["timeToNextBasicNote"] = this.GameStatus.timeToNextBasicNote;
        }

        private void UpdateModJSON()
        {
            if (this.StatusJSON["mod"] == null)
                this.StatusJSON["mod"] = new JSONObject();
            var modJSON = this.StatusJSON["mod"].AsObject;

            modJSON["multiplier"] = this.GameStatus.modifierMultiplier;
            modJSON["obstacles"] = this.GameStatus.modObstacles == null || this.GameStatus.modObstacles == "NoObstacles" ? (JSONNode)new JSONBool(false) : (JSONNode)new JSONString(this.GameStatus.modObstacles);
            modJSON["instaFail"] = this.GameStatus.modInstaFail;
            modJSON["noFail"] = this.GameStatus.modNoFail;
            modJSON["batteryEnergy"] = this.GameStatus.modBatteryEnergy;
            modJSON["batteryLives"] = this.GameStatus.modBatteryEnergy || this.GameStatus.modInstaFail ? (JSONNode)new JSONNumber(this.GameStatus.batteryLives) : (JSONNode)JSONNull.CreateOrGet();
            modJSON["disappearingArrows"] = this.GameStatus.modDisappearingArrows;
            modJSON["noBombs"] = this.GameStatus.modNoBombs;
            modJSON["songSpeed"] = this.GameStatus.modSongSpeed;
            modJSON["songSpeedMultiplier"] = this.GameStatus.songSpeedMultiplier;
            modJSON["noArrows"] = this.GameStatus.modNoArrows;
            modJSON["ghostNotes"] = this.GameStatus.modGhostNotes;
            modJSON["failOnSaberClash"] = this.GameStatus.modFailOnSaberClash;
            modJSON["strictAngles"] = this.GameStatus.modStrictAngles;
            modJSON["fastNotes"] = this.GameStatus.modFastNotes;
            modJSON["smallNotes"] = this.GameStatus.modSmallNotes;
            modJSON["proMode"] = this.GameStatus.modProMode;
            modJSON["zenMode"] = this.GameStatus.modZenMode;
        }

        private void UpdatePlayerSettingsJSON()
        {
            if (this.StatusJSON["playerSettings"] == null)
                this.StatusJSON["playerSettings"] = new JSONObject();
            var playerSettingsJSON = this.StatusJSON["playerSettings"].AsObject;

            playerSettingsJSON["staticLights"] = this.GameStatus.staticLights;
            playerSettingsJSON["leftHanded"] = this.GameStatus.leftHanded;
            playerSettingsJSON["playerHeight"] = this.GameStatus.playerHeight;
            playerSettingsJSON["sfxVolume"] = this.GameStatus.sfxVolume;
            playerSettingsJSON["reduceDebris"] = this.GameStatus.reduceDebris;
            playerSettingsJSON["noHUD"] = this.GameStatus.noHUD;
            playerSettingsJSON["advancedHUD"] = this.GameStatus.advancedHUD;
            playerSettingsJSON["autoRestart"] = this.GameStatus.autoRestart;
            playerSettingsJSON["saberTrailIntensity"] = this.GameStatus.saberTrailIntensity;
            playerSettingsJSON["environmentEffects"] = this.GameStatus.environmentEffects;
            playerSettingsJSON["hideNoteSpawningEffect"] = this.GameStatus.hideNoteSpawningEffect;
        }

        private void UpdateBeatmapEventJSON()
        {
            this.BeatmapEventJSON["type"] = this.GameStatus.beatmapEventType;
            this.BeatmapEventJSON["value"] = this.GameStatus.beatmapEventValue;
        }

        private JSONNode StringOrNull(string str) => str == null ? (JSONNode)JSONNull.CreateOrGet() : (JSONNode)new JSONString(str);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this.thread?.Abort();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                this.disposedValue = true;
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
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
