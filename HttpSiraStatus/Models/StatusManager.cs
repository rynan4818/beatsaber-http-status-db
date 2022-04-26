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
        [Inject]
        public StatusManager(GameStatus gameStatus)
        {
            this._gameStatus = gameStatus;
            this.JsonPool = new ObjectMemoryPool<JSONObject>(null, r => { r.Clear(); }, 20);
            this.UpdateAll();
            this._thread = new Thread(new ThreadStart(this.RaiseSendEvent));
            this._thread.Start();
        }

        public GameStatus GameStatus => this._gameStatus;
        public JSONObject StatusJSON { get; } = new JSONObject();
        public JSONObject NoteCutJSON { get; } = new JSONObject();
        public JSONObject BeatmapEventJSON { get; } = new JSONObject();
        public JSONObject OtherJSON { get; } = new JSONObject();
        public ObjectMemoryPool<JSONObject> JsonPool { get; }
        public ConcurrentQueue<JSONObject> JsonQueue { get; } = new ConcurrentQueue<JSONObject>();


        public event SendEventHandler SendEvent;
        private readonly Thread _thread;
        private bool _disposedValue;
        private readonly GameStatus _gameStatus;

        public void EmitStatusUpdate(ChangedProperty changedProps, BeatSaberEvent e)
        {
            this._gameStatus.updateCause = e.GetDescription();
            if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game) {
                this.UpdateGameJSON();
            }

            if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap) {
                this.UpdateBeatmapJSON();
            }

            if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance) {
                this.UpdatePerformanceJSON();
            }

            if ((changedProps & ChangedProperty.NoteCut) == ChangedProperty.NoteCut) {
                this.UpdateNoteCutJSON();
            }

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

                if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game) {
                    status["game"] = this.StatusJSON["game"];
                }

                if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap) {
                    status["beatmap"] = this.StatusJSON["beatmap"];
                }

                if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance) {
                    status["performance"] = this.StatusJSON["performance"];
                }

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
            if ((changedProps & ChangedProperty.Other) != 0) {
                eventJSON["other"] = this.OtherJSON;
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
            if (this.StatusJSON["game"] == null) {
                this.StatusJSON["game"] = new JSONObject();
            }

            var gameJSON = this.StatusJSON["game"].AsObject;

            gameJSON["pluginVersion"] = Plugin.PluginVersion;
            gameJSON["gameVersion"] = Plugin.GameVersion;
            gameJSON["scene"] = this.StringOrNull(this._gameStatus.scene);
            gameJSON["mode"] = this.StringOrNull(this._gameStatus.mode);
        }

        private void UpdateBeatmapJSON()
        {
            if (this._gameStatus.songName == null) {
                this.StatusJSON["beatmap"] = null;
                return;
            }

            if (this.StatusJSON["beatmap"] == null) {
                this.StatusJSON["beatmap"] = new JSONObject();
            }

            var beatmapJSON = this.StatusJSON["beatmap"].AsObject;

            beatmapJSON["songName"] = this.StringOrNull(this._gameStatus.songName);
            beatmapJSON["songSubName"] = this.StringOrNull(this._gameStatus.songSubName);
            beatmapJSON["songAuthorName"] = this.StringOrNull(this._gameStatus.songAuthorName);
            beatmapJSON["levelAuthorName"] = this.StringOrNull(this._gameStatus.levelAuthorName);
            beatmapJSON["songCover"] = string.IsNullOrEmpty(this._gameStatus.songCover) ? JSONNull.CreateOrGet() : new JSONString(this._gameStatus.songCover);
            beatmapJSON["songHash"] = this.StringOrNull(this._gameStatus.songHash);
            beatmapJSON["levelId"] = this.StringOrNull(this._gameStatus.levelId);
            beatmapJSON["songBPM"] = this._gameStatus.songBPM;
            beatmapJSON["noteJumpSpeed"] = this._gameStatus.noteJumpSpeed;
            beatmapJSON["noteJumpStartBeatOffset"] = this._gameStatus.noteJumpStartBeatOffset;
            beatmapJSON["songTimeOffset"] = new JSONNumber(this._gameStatus.songTimeOffset);
            beatmapJSON["start"] = this._gameStatus.start == 0 ? JSONNull.CreateOrGet() : new JSONNumber(this._gameStatus.start);
            beatmapJSON["paused"] = this._gameStatus.paused == 0 ? JSONNull.CreateOrGet() : new JSONNumber(this._gameStatus.paused);
            beatmapJSON["length"] = new JSONNumber(this._gameStatus.length);
            beatmapJSON["difficulty"] = this.StringOrNull(this._gameStatus.difficulty);
            beatmapJSON["difficultyEnum"] = this.StringOrNull(this._gameStatus.difficultyEnum);
            beatmapJSON["characteristic "] = this.StringOrNull(this._gameStatus.characteristic);
            beatmapJSON["notesCount"] = this._gameStatus.notesCount;
            beatmapJSON["bombsCount"] = this._gameStatus.bombsCount;
            beatmapJSON["obstaclesCount"] = this._gameStatus.obstaclesCount;
            beatmapJSON["maxScore"] = this._gameStatus.maxScore;
            beatmapJSON["maxRank"] = this._gameStatus.maxRank;
            beatmapJSON["environmentName"] = this._gameStatus.environmentName;

            if (beatmapJSON["color"] == null) {
                beatmapJSON["color"] = new JSONObject();
            }
            var colorJSON = beatmapJSON["color"].AsObject;

            this.UpdateColor(this._gameStatus.colorSaberA, colorJSON, "saberA");
            this.UpdateColor(this._gameStatus.colorSaberB, colorJSON, "saberB");
            this.UpdateColor(this._gameStatus.colorEnvironment0, colorJSON, "environment0");
            this.UpdateColor(this._gameStatus.colorEnvironment1, colorJSON, "environment1");
            this.UpdateColor(this._gameStatus.colorEnvironmentBoost0, colorJSON, "environment0Boost");
            this.UpdateColor(this._gameStatus.colorEnvironmentBoost1, colorJSON, "environment1Boost");
            this.UpdateColor(this._gameStatus.colorObstacle, colorJSON, "obstacle");
        }

        private void UpdateColor(Color? color, JSONObject parent, string key)
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
            if (this._gameStatus.start == 0) {
                this.StatusJSON["performance"] = null;
                return;
            }

            if (this.StatusJSON["performance"] == null) {
                this.StatusJSON["performance"] = new JSONObject();
            }

            var performanceJSON = this.StatusJSON["performance"].AsObject;

            performanceJSON["rawScore"].AsInt = this._gameStatus.rawScore;
            performanceJSON["score"].AsInt = this._gameStatus.score;
            performanceJSON["currentMaxScore"].AsInt = this._gameStatus.currentMaxScore;
            performanceJSON["relativeScore"].AsFloat = this._gameStatus.relativeScore;
            performanceJSON["rank"] = this._gameStatus.rank;
            performanceJSON["passedNotes"].AsInt = this._gameStatus.passedNotes;
            performanceJSON["hitNotes"].AsInt = this._gameStatus.hitNotes;
            performanceJSON["missedNotes"].AsInt = this._gameStatus.missedNotes;
            performanceJSON["lastNoteScore"].AsInt = this._gameStatus.lastNoteScore;
            performanceJSON["passedBombs"].AsInt = this._gameStatus.passedBombs;
            performanceJSON["hitBombs"].AsInt = this._gameStatus.hitBombs;
            performanceJSON["combo"].AsInt = this._gameStatus.combo;
            performanceJSON["maxCombo"].AsInt = this._gameStatus.maxCombo;
            performanceJSON["multiplier"].AsInt = this._gameStatus.multiplier;
            performanceJSON["multiplierProgress"].AsFloat = this._gameStatus.multiplierProgress;
            performanceJSON["batteryEnergy"] = this._gameStatus.modBatteryEnergy || this._gameStatus.modInstaFail ? new JSONNumber(this._gameStatus.batteryEnergy) : JSONNull.CreateOrGet();
            performanceJSON["energy"].AsFloat = this._gameStatus.energy;
            performanceJSON["softFailed"].AsBool = this._gameStatus.softFailed;
            performanceJSON["currentSongTime"].AsInt = this._gameStatus.currentSongTime;
        }

        private void UpdateNoteCutJSON()
        {
            this.NoteCutJSON["noteID"] = this._gameStatus.noteID;
            this.NoteCutJSON["noteType"] = this.StringOrNull(this._gameStatus.noteType);
            this.NoteCutJSON["noteCutDirection"] = this.StringOrNull(this._gameStatus.noteCutDirection);
            this.NoteCutJSON["noteLine"] = this._gameStatus.noteLine;
            this.NoteCutJSON["noteLayer"] = this._gameStatus.noteLayer;
            this.NoteCutJSON["speedOK"] = this._gameStatus.speedOK;
            this.NoteCutJSON["directionOK"] = this._gameStatus.noteType == "Bomb" ? JSONNull.CreateOrGet() : new JSONBool(this._gameStatus.directionOK);
            this.NoteCutJSON["saberTypeOK"] = this._gameStatus.noteType == "Bomb" ? JSONNull.CreateOrGet() : new JSONBool(this._gameStatus.saberTypeOK);
            this.NoteCutJSON["wasCutTooSoon"] = this._gameStatus.wasCutTooSoon;
            this.NoteCutJSON["initialScore"] = this._gameStatus.initialScore == -1 ? JSONNull.CreateOrGet() : new JSONNumber(this._gameStatus.initialScore);
            this.NoteCutJSON["finalScore"] = this._gameStatus.finalScore == -1 ? JSONNull.CreateOrGet() : new JSONNumber(this._gameStatus.finalScore);
            this.NoteCutJSON["cutDistanceScore"] = this._gameStatus.cutDistanceScore == -1 ? JSONNull.CreateOrGet() : new JSONNumber(this._gameStatus.cutDistanceScore);
            this.NoteCutJSON["swingRating"] = this._gameStatus.swingRating;
            this.NoteCutJSON["beforSwingRating"] = this._gameStatus.beforSwingRating;
            this.NoteCutJSON["afterSwingRating"] = this._gameStatus.afterSwingRating;
            this.NoteCutJSON["multiplier"] = this._gameStatus.cutMultiplier;
            this.NoteCutJSON["saberSpeed"] = this._gameStatus.saberSpeed;
            if (!this.NoteCutJSON["saberDir"].IsArray) {
                this.NoteCutJSON["saberDir"] = new JSONArray();
            }

            this.NoteCutJSON["saberDir"][0].AsFloat = this._gameStatus.saberDirX;
            this.NoteCutJSON["saberDir"][1].AsFloat = this._gameStatus.saberDirY;
            this.NoteCutJSON["saberDir"][2].AsFloat = this._gameStatus.saberDirZ;
            this.NoteCutJSON["saberType"] = this.StringOrNull(this._gameStatus.saberType);
            this.NoteCutJSON["timeDeviation"] = this._gameStatus.timeDeviation;
            this.NoteCutJSON["cutDirectionDeviation"] = this._gameStatus.cutDirectionDeviation;
            if (!this.NoteCutJSON["cutPoint"].IsArray) {
                this.NoteCutJSON["cutPoint"] = new JSONArray();
            }

            this.NoteCutJSON["cutPoint"][0].AsFloat = this._gameStatus.cutPointX;
            this.NoteCutJSON["cutPoint"][1].AsFloat = this._gameStatus.cutPointY;
            this.NoteCutJSON["cutPoint"][2].AsFloat = this._gameStatus.cutPointZ;
            if (!this.NoteCutJSON["cutNormal"].IsArray) {
                this.NoteCutJSON["cutNormal"] = new JSONArray();
            }

            this.NoteCutJSON["cutNormal"][0].AsFloat = this._gameStatus.cutNormalX;
            this.NoteCutJSON["cutNormal"][1].AsFloat = this._gameStatus.cutNormalY;
            this.NoteCutJSON["cutNormal"][2].AsFloat = this._gameStatus.cutNormalZ;
            this.NoteCutJSON["cutDistanceToCenter"] = this._gameStatus.cutDistanceToCenter;
            this.NoteCutJSON["timeToNextBasicNote"] = this._gameStatus.timeToNextBasicNote;
        }

        private void UpdateModJSON()
        {
            if (this.StatusJSON["mod"] == null) {
                this.StatusJSON["mod"] = new JSONObject();
            }

            var modJSON = this.StatusJSON["mod"].AsObject;

            modJSON["multiplier"] = this._gameStatus.modifierMultiplier;
            modJSON["obstacles"] = this._gameStatus.modObstacles == null || this._gameStatus.modObstacles == "NoObstacles" ? new JSONBool(false) : new JSONString(this._gameStatus.modObstacles);
            modJSON["instaFail"] = this._gameStatus.modInstaFail;
            modJSON["noFail"] = this._gameStatus.modNoFail;
            modJSON["batteryEnergy"] = this._gameStatus.modBatteryEnergy;
            modJSON["batteryLives"] = this._gameStatus.modBatteryEnergy || this._gameStatus.modInstaFail ? new JSONNumber(this._gameStatus.batteryLives) : JSONNull.CreateOrGet();
            modJSON["disappearingArrows"] = this._gameStatus.modDisappearingArrows;
            modJSON["noBombs"] = this._gameStatus.modNoBombs;
            modJSON["songSpeed"] = this._gameStatus.modSongSpeed;
            modJSON["songSpeedMultiplier"] = this._gameStatus.songSpeedMultiplier;
            modJSON["noArrows"] = this._gameStatus.modNoArrows;
            modJSON["ghostNotes"] = this._gameStatus.modGhostNotes;
            modJSON["failOnSaberClash"] = this._gameStatus.modFailOnSaberClash;
            modJSON["strictAngles"] = this._gameStatus.modStrictAngles;
            modJSON["fastNotes"] = this._gameStatus.modFastNotes;
            modJSON["smallNotes"] = this._gameStatus.modSmallNotes;
            modJSON["proMode"] = this._gameStatus.modProMode;
            modJSON["zenMode"] = this._gameStatus.modZenMode;
        }

        private void UpdatePlayerSettingsJSON()
        {
            if (this.StatusJSON["playerSettings"] == null) {
                this.StatusJSON["playerSettings"] = new JSONObject();
            }

            var playerSettingsJSON = this.StatusJSON["playerSettings"].AsObject;

            playerSettingsJSON["staticLights"] = this._gameStatus.staticLights;
            playerSettingsJSON["leftHanded"] = this._gameStatus.leftHanded;
            playerSettingsJSON["playerHeight"] = this._gameStatus.playerHeight;
            playerSettingsJSON["sfxVolume"] = this._gameStatus.sfxVolume;
            playerSettingsJSON["reduceDebris"] = this._gameStatus.reduceDebris;
            playerSettingsJSON["noHUD"] = this._gameStatus.noHUD;
            playerSettingsJSON["advancedHUD"] = this._gameStatus.advancedHUD;
            playerSettingsJSON["autoRestart"] = this._gameStatus.autoRestart;
            playerSettingsJSON["saberTrailIntensity"] = this._gameStatus.saberTrailIntensity;
            playerSettingsJSON["environmentEffects"] = this._gameStatus.environmentEffects;
            playerSettingsJSON["hideNoteSpawningEffect"] = this._gameStatus.hideNoteSpawningEffect;
        }

        private void UpdateBeatmapEventJSON()
        {
            this.BeatmapEventJSON["type"] = this._gameStatus.beatmapEventType;
            this.BeatmapEventJSON["value"] = this._gameStatus.beatmapEventValue;
        }

        private JSONNode StringOrNull(string str)
        {
            return str == null ? JSONNull.CreateOrGet() : new JSONString(str);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    try {
                        this._thread?.Abort();
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
        }
    }
}
