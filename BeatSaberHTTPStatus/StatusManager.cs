using System;
using UnityEngine;
using SimpleJSON;
using System.Threading.Tasks;
using BeatSaberHTTPStatus.Interfaces;
using Zenject;
using System.Collections.Concurrent;
using BeatSaberHTTPStatus.Util;

namespace BeatSaberHTTPStatus {
	public class StatusManager : IStatusManager
	{
		[Inject]
		public GameStatus GameStatus { get; }

		private JSONObject _statusJSON = new JSONObject();
		public JSONObject StatusJSON {
			get {return _statusJSON;}
		}

		private JSONObject _noteCutJSON = new JSONObject();
		public JSONObject NoteCutJSON {
			get {return _noteCutJSON;}
		}

		private JSONObject _beatmapEventJSON = new JSONObject();
		public JSONObject BeatmapEventJSON {
			get {return _beatmapEventJSON;}
		}

		public ConcurrentQueue<JSONObject> JsonQueue { get; } = new ConcurrentQueue<JSONObject>();
        [Inject]
		void Constractor()
        {
			UpdateAll();
		}

		public void EmitStatusUpdate(ChangedProperty changedProps, BeatSaberEvent e) {
			Plugin.Logger.Debug($"{changedProps}");
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
			var eventJSON = new JSONObject();
			eventJSON["event"] = e.GetDescription();

			if ((changedProps & (ChangedProperty.Game | ChangedProperty.Beatmap | ChangedProperty.Performance | ChangedProperty.Mod))
				== (ChangedProperty.Game | ChangedProperty.Beatmap | ChangedProperty.Performance | ChangedProperty.Mod)) {
				eventJSON["status"] = this.StatusJSON;
			}
			else {
				var status = new JSONObject();

				if ((changedProps & ChangedProperty.Game) == ChangedProperty.Game) status["game"] = this.StatusJSON["game"];
				if ((changedProps & ChangedProperty.Beatmap) == ChangedProperty.Beatmap) status["beatmap"] = this.StatusJSON["beatmap"];
				if ((changedProps & ChangedProperty.Performance) == ChangedProperty.Performance) status["performance"] = this.StatusJSON["performance"];
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

		private void UpdateAll() {
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

		private void UpdateGameJSON() {
			if (_statusJSON["game"] == null) _statusJSON["game"] = new JSONObject();
			JSONObject gameJSON = (JSONObject) _statusJSON["game"];

			gameJSON["pluginVersion"] = BeatSaberHTTPStatus.Plugin.PluginVersion;
			gameJSON["gameVersion"] = BeatSaberHTTPStatus.Plugin.GameVersion;
			gameJSON["scene"] = stringOrNull(GameStatus.scene);
			gameJSON["mode"] = stringOrNull(GameStatus.mode == null ? null : (GameStatus.partyMode ? "Party" : "Solo") + GameStatus.mode);
		}

		private void UpdateBeatmapJSON() {
			if (GameStatus.songName == null) {
				_statusJSON["beatmap"] = null;
				return;
			}

			if (_statusJSON["beatmap"] == null) _statusJSON["beatmap"] = new JSONObject();
			JSONObject beatmapJSON = (JSONObject) _statusJSON["beatmap"];

			beatmapJSON["songName"] = stringOrNull(GameStatus.songName);
			beatmapJSON["songSubName"] = stringOrNull(GameStatus.songSubName);
			beatmapJSON["songAuthorName"] = stringOrNull(GameStatus.songAuthorName);
			beatmapJSON["levelAuthorName"] = stringOrNull(GameStatus.levelAuthorName);
			beatmapJSON["songCover"] = String.IsNullOrEmpty(GameStatus.songCover) ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONString(GameStatus.songCover);
			beatmapJSON["songHash"] = stringOrNull(GameStatus.songHash);
			beatmapJSON["levelId"] = stringOrNull(GameStatus.levelId);
			beatmapJSON["songBPM"] = GameStatus.songBPM;
			beatmapJSON["noteJumpSpeed"] = GameStatus.noteJumpSpeed;
			beatmapJSON["songTimeOffset"] = new JSONNumber(GameStatus.songTimeOffset);
			beatmapJSON["start"] = GameStatus.start == 0 ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONNumber(GameStatus.start);
			beatmapJSON["paused"] = GameStatus.paused == 0 ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONNumber(GameStatus.paused);
			beatmapJSON["length"] = new JSONNumber(GameStatus.length);
			beatmapJSON["difficulty"] = stringOrNull(GameStatus.difficulty);
			beatmapJSON["notesCount"] = GameStatus.notesCount;
			beatmapJSON["bombsCount"] = GameStatus.bombsCount;
			beatmapJSON["obstaclesCount"] = GameStatus.obstaclesCount;
			beatmapJSON["maxScore"] = GameStatus.maxScore;
			beatmapJSON["maxRank"] = GameStatus.maxRank;
			beatmapJSON["environmentName"] = GameStatus.environmentName;
		}

		private void UpdatePerformanceJSON() {
			if (GameStatus.start == 0) {
				_statusJSON["performance"] = null;
				return;
			}

			if (_statusJSON["performance"] == null) _statusJSON["performance"] = new JSONObject();
			JSONObject performanceJSON = (JSONObject) _statusJSON["performance"];

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
			performanceJSON["batteryEnergy"] = GameStatus.modBatteryEnergy || GameStatus.modInstaFail ? (JSONNode) new JSONNumber(GameStatus.batteryEnergy) : (JSONNode) JSONNull.CreateOrGet();
			performanceJSON["energy"] = new JSONNumber(this.GameStatus.energy);
		}

		private void UpdateNoteCutJSON() {
			_noteCutJSON["noteID"] = GameStatus.noteID;
			_noteCutJSON["noteType"] = stringOrNull(GameStatus.noteType);
			_noteCutJSON["noteCutDirection"] = stringOrNull(GameStatus.noteCutDirection);
			_noteCutJSON["noteLine"] = GameStatus.noteLine;
			_noteCutJSON["noteLayer"] = GameStatus.noteLayer;
			_noteCutJSON["speedOK"] = GameStatus.speedOK;
			_noteCutJSON["directionOK"] = GameStatus.noteType == "Bomb" ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONBool(GameStatus.directionOK);
			_noteCutJSON["saberTypeOK"] = GameStatus.noteType == "Bomb" ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONBool(GameStatus.saberTypeOK);
			_noteCutJSON["wasCutTooSoon"] = GameStatus.wasCutTooSoon;
			_noteCutJSON["initialScore"] = GameStatus.initialScore == -1 ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONNumber(GameStatus.initialScore);
			_noteCutJSON["finalScore"] = GameStatus.finalScore == -1 ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONNumber(GameStatus.finalScore);
			_noteCutJSON["cutDistanceScore"] = GameStatus.cutDistanceScore == -1 ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONNumber(GameStatus.cutDistanceScore);
			_noteCutJSON["swingRating"] = GameStatus.swingRating;
			_noteCutJSON["multiplier"] = GameStatus.cutMultiplier;
			_noteCutJSON["saberSpeed"] = GameStatus.saberSpeed;
			if (!_noteCutJSON["saberDir"].IsArray) _noteCutJSON["saberDir"] = new JSONArray();
			_noteCutJSON["saberDir"][0] = GameStatus.saberDirX;
			_noteCutJSON["saberDir"][1] = GameStatus.saberDirY;
			_noteCutJSON["saberDir"][2] = GameStatus.saberDirZ;
			_noteCutJSON["saberType"] = stringOrNull(GameStatus.saberType);
			_noteCutJSON["timeDeviation"] = GameStatus.timeDeviation;
			_noteCutJSON["cutDirectionDeviation"] = GameStatus.cutDirectionDeviation;
			if (!_noteCutJSON["cutPoint"].IsArray) _noteCutJSON["cutPoint"] = new JSONArray();
			_noteCutJSON["cutPoint"][0] = GameStatus.cutPointX;
			_noteCutJSON["cutPoint"][1] = GameStatus.cutPointY;
			_noteCutJSON["cutPoint"][2] = GameStatus.cutPointZ;
			if (!_noteCutJSON["cutNormal"].IsArray) _noteCutJSON["cutNormal"] = new JSONArray();
			_noteCutJSON["cutNormal"][0] = GameStatus.cutNormalX;
			_noteCutJSON["cutNormal"][1] = GameStatus.cutNormalY;
			_noteCutJSON["cutNormal"][2] = GameStatus.cutNormalZ;
			_noteCutJSON["cutDistanceToCenter"] = GameStatus.cutDistanceToCenter;
			_noteCutJSON["timeToNextBasicNote"] = GameStatus.timeToNextBasicNote;
		}

		private void UpdateModJSON() {
			if (_statusJSON["mod"] == null) _statusJSON["mod"] = new JSONObject();
			JSONObject modJSON = (JSONObject) _statusJSON["mod"];

			modJSON["multiplier"] = GameStatus.modifierMultiplier;
			modJSON["obstacles"] = GameStatus.modObstacles == null || GameStatus.modObstacles == "NoObstacles" ? (JSONNode) new JSONBool(false) : (JSONNode) new JSONString(GameStatus.modObstacles);
			modJSON["instaFail"] = GameStatus.modInstaFail;
			modJSON["noFail"] = GameStatus.modNoFail;
			modJSON["batteryEnergy"] = GameStatus.modBatteryEnergy;
			modJSON["batteryLives"] = GameStatus.modBatteryEnergy || GameStatus.modInstaFail ? (JSONNode) new JSONNumber(GameStatus.batteryLives) : (JSONNode) JSONNull.CreateOrGet();
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

		private void UpdatePlayerSettingsJSON() {
			if (_statusJSON["playerSettings"] == null) _statusJSON["playerSettings"] = new JSONObject();
			JSONObject playerSettingsJSON = (JSONObject) _statusJSON["playerSettings"];

			playerSettingsJSON["staticLights"] = GameStatus.staticLights;
			playerSettingsJSON["leftHanded"] = GameStatus.leftHanded;
			playerSettingsJSON["playerHeight"] = GameStatus.playerHeight;
			playerSettingsJSON["sfxVolume"] = GameStatus.sfxVolume;
			playerSettingsJSON["reduceDebris"] = GameStatus.reduceDebris;
			playerSettingsJSON["noHUD"] = GameStatus.noHUD;
			playerSettingsJSON["advancedHUD"] = GameStatus.advancedHUD;
			playerSettingsJSON["autoRestart"] = GameStatus.autoRestart;
		}

		private void UpdateBeatmapEventJSON() {
			_beatmapEventJSON["type"] = GameStatus.beatmapEventType;
			_beatmapEventJSON["value"] = GameStatus.beatmapEventValue;
		}

		private JSONNode stringOrNull(string str) {
			return str == null ? (JSONNode) JSONNull.CreateOrGet() : (JSONNode) new JSONString(str);
		}
	}
}
