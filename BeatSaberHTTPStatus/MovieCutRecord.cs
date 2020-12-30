using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;
using System.Data.SQLite;

namespace BeatSaberHTTPStatus
{
	public class NoteScore
	{
		// Event
		public string bs_event { get; set; } = "";				 //イベント名
		public long time { get; set; } = 0; 					 //イベント発生時間(UNIX time[ms])
		public long? cutTime { get; set; } = null;				 //noteCut時間
		// Performance
		public int score { get; set; } = 0; 					 //現在のスコア
		public int currentMaxScore { get; set; } = 0;			 //現在処理したノーツで達成可能な最高スコア
		public string rank { get; set; } = "E"; 				 //現在のランク
		public int passedNotes { get; set; } = 0;				 //現在処理したノーツ数
		public int hitNotes { get; set; } = 0;					 //現在のヒットしたノーツ数
		public int missedNotes { get; set; } = 0;				 //現在のミスしたノーツ・爆弾数
		public int lastNoteScore { get; set; } = 0; 			 //未実装
		public int passedBombs { get; set; } = 0;				 //現在処理した爆弾数
		public int hitBombs { get; set; } = 0;					 //現在のヒットした爆弾数
		public int combo { get; set; } = 0; 					 //現在のコンボ数
		public int maxCombo { get; set; } = 0;					 //現在取得した最大コンボ数
		public int multiplier { get; set; } = 0;				 //現在のコンボ乗数[1,2,4,8]
		public float multiplierProgress { get; set; } = 0;		 //現在のコンボ乗数の進行[0..1]
		public int? batteryEnergy { get; set; } = 1;			 //現在のバッテリー寿命の残り。バッテリーエネルギーとインスタ障害が無効になっている場合はnull。

		// NoteCut
		public int? noteID { get; set; } = null;				 //ノーツ番号
		public string noteType { get; set; } = null;			 //"NoteA" | "NoteB" | "GhostNote" | "Bomb" //ノーツの種類
		public string noteCutDirection { get; set; } = null;	 //"Up" | "Down" | "Left" | "Right" | "UpLeft" | "UpRight" | "DownLeft" | "DownRight" | "Any" | "None" //ノーツの矢印種類
		public int? noteLine { get; set; } = null;				 //左から右へのノーツの水平位置[0..3] 
		public int? noteLayer { get; set; } = null; 			 //下から上へのノーツの垂直位置[0..2] 
		public bool? speedOK { get; set; } = null;				 //カット速度は十分に速かった
		public bool? directionOK { get; set; } = null;			 //正しい方向でノーツがカットされた。爆弾の場合はnull。
		public bool? saberTypeOK { get; set; } = null;			 //正しいセイバーでノーツがカットされた。爆弾の場合はnull。
		public bool? wasCutTooSoon { get; set; } = null;		 //ノーツのカットが早すぎる
		public int? initialScore { get; set; } = null;			 //カット前のスイングのスコアとノーツ中心カットの合計[max85]。爆弾の場合はnull。
		public int? beforeScore { get; set; } = null;            //カット前のスイングのスコア[max70]。爆弾の場合はnull。
		public int? afterScore { get; set; } = null;			 //カット後のスイングのスコア[max30]。爆弾の場合はnull。
		public int? cutDistanceScore { get; set; } = null;		 //ノーツ中心カットのスコア[max15]。  爆弾の場合はnull。
		public int? finalScore { get; set; } = null;			 //カット全体の乗数なしのスコア。爆弾の場合はnull。
		public int? cutMultiplier { get; set; } = null; 		 //カット時のコンボ乗数
		public float? saberSpeed { get; set; } = null;			 //ノーツがカットされたときのセイバーの速度
		public float? saberDirX { get; set; } = null;			 //ノーツがカットされたときにセイバーが移動した方向[X]
		public float? saberDirY { get; set; } = null;			 //同上[Y]
		public float? saberDirZ { get; set; } = null;			 //同上[Z]
		public string saberType { get; set; } = null;			 //"SaberA" | "SaberB" 、 このノーツをカットするために使用されるセイバー
		public float? swingRating { get; set; } = null; 		 //ゲームのスイング評価。カット前の評価。爆弾の場合は-1。
		public float? swingRatingFullyCut { get; set; } = null;  //ゲームのスイング評価。カット後の評価を使用します。爆弾の場合は-1。
		public float? timeDeviation { get; set; } = null;		 //ノーツをカットするのに最適な時間からの秒単位の時間オフセット
		public float? cutDirectionDeviation { get; set; } = null;//度単位の完全なカット角度からのオフセット
		public float? cutPointX { get; set; } = null;			 //ノーツの中心に最も近いカット平面上のポイントの位置
		public float? cutPointY { get; set; } = null;			 //同上[Y]
		public float? cutPointZ { get; set; } = null;			 //同上[Z]
		public float? cutNormalX { get; set; } = null;			 //カットする理想的な平面の法線
		public float? cutNormalY { get; set; } = null;			 //同上[Y]
		public float? cutNormalZ { get; set; } = null;			 //同上[Z]
		public float? cutDistanceToCenter { get; set; } = null;  //ノーツの中心からカット平面までの距離
		public float? timeToNextBasicNote { get; set; } = null;  //次のノーツまでの時間（秒）
	}

	public class MovieCutRecord
	{
		//イベント名称
		private const string menu_event_name = "menu";
		private const string songStart_event_name = "songStart";
		private const string obstacleEnter_event_name = "obstacleEnter";
		private const string obstacleExit_event_name = "obstacleExit";
		private const string pause_event_name = "pause";
		private const string resume_event_name = "resume";
		private const string bombCut_event_name = "bombCut";
		private const string noteCut_event_name = "noteCut";
		private const string noteFullyCut_event_name = "noteFullyCut";
		private const string bombMissed_event_name = "bombMissed";
		private const string noteMissed_event_name = "noteMissed";
		private const string scoreChanged_event_name = "scoreChanged";
		private const string finished_event_name = "finished";
		private const string failed_event_name = "failed";
		private const string beatmapEvent_event_name = "beatmapEvent";

		// Default setting
		private const int obstacleEventCount = 100; 					//obstacleイベント分
		private const int initNoteScoreSize  = 2000;					//noteScore配列初期化サイズ (必要な配列サイズはノーツ数＋爆弾数＋obstacleイベント数)
		private const int addArraySize = 100;                           //配列追加時余裕分
		private static readonly string defaultDbFile = System.IO.Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "beatsaber.db");  //データベースファイル初期値
		private string dbFile;											//データベースファイル名

		// Setting file
		private string settingFile = System.IO.Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "movie_cut_record.json"); //設定ファイル名
		private Encoding encUTF8 = new UTF8Encoding(false);

		//http_statusイベント送信許可
		private bool http_scenechange { get; set; } = true;  //songStart,finished,failed,menu,pause,resume
		private bool http_scorechanged { get; set; } = true; //scoreChanged
		private bool http_notecut { get; set; } = true;      //noteCut
		private bool http_notefullycut { get; set; } = true; //noteFullyCut
		private bool http_notemissed { get; set; } = true;   //noteMissed
		private bool http_bombcut { get; set; } = true;      //bombCut
		private bool http_bombmissed { get; set; } = true;   //bombMissed
		private bool http_beatmapevent { get; set; } = true; //beatmapEvent
		private bool http_obstacle { get; set; } = true; 	 //obstacleEnter,obstacleExit
		private bool db_notes_score = true; 				 //ノーツ毎のスコア記録許可
		private bool gc_collect = true; 					 //songStart、menuイベント時にGC実行

		// MovieCutRecord記録用
		private bool song = false;										   //譜面プレイ中フラグ
		private bool end = false;										   //finished又はfailed終了フラグ
		private int pause = 0;											   //プレイ中にpauseした回数
		private string cleared = "";									   //プレイ終了イベント
		private long start_time = 0;									   //プレイ開始時間(UNIX time[ms])
		private long end_time = 0;										   //プレイ終了時間(UNIX time[ms])

		// NoteScore記録用
		private NoteScore[] noteScores = new NoteScore[initNoteScoreSize]; //ノーツ毎のスコア格納用配列
		private long[] noteCutTime = new long[initNoteScoreSize];		   //noteCutの時間格納用配列
		private float[] cutSwingRating = new float[initNoteScoreSize];	   //noteCut時のswingRating格納用配列
		private int noteScoresIdx = 0;									   //notescoresの最終書込み添字
		private int noteCutTimeIdx = 0; 								   //noteCutTieの最終書込み添字
		private int initSize = 0;										   //配列の初期化済みサイズ

		// StatusObject[game]
		private string scene = menu_event_name;					//プレイ中画面の現在のシーン
		private bool partyMode = false; 						//Partyモード有無
		private string mode = null; 							//プレイモード

		// StatusObject[beatmap]
		private string songName = null; 						//曲名
		private string songSubName = null;						//曲のサブタイトル
		private string songAuthorName = null;					//曲の作者
		private string levelAuthorName = null;					//譜面の作者
		private string songHash = null; 						//譜面ID(SHA-1ハッシュ値)
		private string levelId = null;                          //譜面のRawレベル。全て難易度で同じ
		private float songBPM;									//曲のBPM
		private float noteJumpSpeed;							//譜面のNJS
		private long songTimeOffset = 0;						//譜面開始オフセット値(取得出来ていない？)
		private long length = 0;								//譜面の長さ[ms]
		private long start = 0; 								//譜面プレイ開始時の時間。再開時に更新。(UNIX time[ms])
		private long paused = 0;								//一時停止時の時間(UNIX time[ms])
		private string difficulty = null;						//譜面の難易度
		private int notesCount = 0; 							//譜面のノーツ数
		private int bombsCount = 0; 							//譜面の爆弾数
		private int obstaclesCount = 0; 						//譜面の壁の数
		private int maxScore = 0;								//現在のModでの最大スコア
		private string maxRank = "E";							//現在のModでの最大ランク
		private string environmentName = null;					//譜面の要求環境

		// StatusObject[performance]
		private int score = 0;									//現在のスコア
		private int currentMaxScore = 0;						//現在のノーツ数で達成可能な最大スコア
		private string rank = "E";								//現在のランク
		private int passedNotes = 0;							//現在処理したノーツ数
		private int hitNotes = 0;								//現在ヒットしたノーツ数
		private int missedNotes = 0;							//現在ミスしたノーツ数
		private int lastNoteScore = 0;							//（取得出来ていない？）
		private int passedBombs = 0;							//現在処理した爆弾数
		private int hitBombs = 0;								//現在ヒットした爆弾数
		private int combo = 0;									//現在のコンボ数
		private int maxCombo = 0;								//現在の最大コンボ数
		private int multiplier = 0; 							//現在のコンボ乗数
		private float multiplierProgress = 0;					//現在のコンボ乗数の進行具合
		private int batteryEnergy = 1;							//現在のライフ残量

		// StatusObject[mods]
		private float modifierMultiplier = 1f;					//Mod乗数
		private string modObstacles = "All";					//壁の有無
		private bool modInstaFail = false;						//ノーミス
		private bool modNoFail = false; 						//失敗無し
		private bool modBatteryEnergy = false;					//ライフ残量？
		private int batteryLives = 1;							//最大ライフ残量(DB未記録)
		private bool modDisappearingArrows = false; 			//消える矢印
		private bool modNoBombs = false;						//爆弾無し
		private string modSongSpeed = "Normal"; 				//曲の速度
		private float songSpeedMultiplier = 1f; 				//曲の速度のMod乗数
		private bool modNoArrows = false;						//矢印無し
		private bool modGhostNotes = false; 					//ゴーストノーツ
		private bool modFailOnSaberClash = false;				//セイバークラッシュで失敗？（Hidden)
		private bool modStrictAngles = false;					//厳密な角度(Hidden)
		private bool modFastNotes = false;						//Does something (Hidden)

		// StatusObject[playerSettings]
		private bool staticLights = false;						//静的ライト
		private bool leftHanded = false;						//左利き
		private float playerHeight = 1.7f;						//プレイヤーの高さ
		private bool reduceDebris = false;						//ノーツカット時の破片有無
		private bool noHUD = false; 							//テキストとHUD無し
		private bool advancedHUD = false;						//Advanced HUD
		private bool autoRestart = false;						//失敗時に自動リスタート

		public void BeatsaberEvent(GameStatus gameStatus, string bs_event)
		{
			if (bs_event == songStart_event_name)
			{
				start_time = Plugin.GetCurrentTime();
				song = true;
				end = false;
				pause = 0;
				cleared = bs_event;
				scene = gameStatus.scene;
				mode = gameStatus.mode;
				partyMode = gameStatus.partyMode;
				// Beatmap
				songName = gameStatus.songName;
				songSubName = gameStatus.songSubName;
				songAuthorName = gameStatus.songAuthorName;
				levelAuthorName = gameStatus.levelAuthorName;
				songHash = gameStatus.songHash;
				levelId = gameStatus.levelId;
				songBPM = gameStatus.songBPM;
				noteJumpSpeed = gameStatus.noteJumpSpeed;
				songTimeOffset = gameStatus.songTimeOffset;
				length = gameStatus.length;
				start = gameStatus.start;
				paused = gameStatus.paused;
				difficulty = gameStatus.difficulty;
				notesCount = gameStatus.notesCount;
				bombsCount = gameStatus.bombsCount;
				obstaclesCount = gameStatus.obstaclesCount;
				maxScore = gameStatus.maxScore;
				maxRank = gameStatus.maxRank;
				environmentName = gameStatus.environmentName;

				// Performance
				score = 0;
				currentMaxScore = 0;
				rank = "E";
				passedNotes = 0;
				hitNotes = 0;
				missedNotes = 0;
				lastNoteScore = 0;
				passedBombs = 0;
				hitBombs = 0;
				combo = 0;
				maxCombo = 0;
				multiplier = 0;
				multiplierProgress = 0;
				batteryEnergy = 1;

				// Mods
				modifierMultiplier = gameStatus.modifierMultiplier;
				modObstacles = gameStatus.modObstacles;
				modInstaFail = gameStatus.modInstaFail;
				modNoFail = gameStatus.modNoFail;
				modBatteryEnergy = gameStatus.modBatteryEnergy;
				batteryLives = gameStatus.batteryLives;
				modDisappearingArrows = gameStatus.modDisappearingArrows;
				modNoBombs = gameStatus.modNoBombs;
				modSongSpeed = gameStatus.modSongSpeed;
				songSpeedMultiplier = gameStatus.songSpeedMultiplier;
				modNoArrows = gameStatus.modNoArrows;
				modGhostNotes = gameStatus.modGhostNotes;
				modFailOnSaberClash = gameStatus.modFailOnSaberClash;
				modStrictAngles = gameStatus.modStrictAngles;
				modFastNotes = gameStatus.modFastNotes;

				// Player settings
				staticLights = gameStatus.staticLights;
				leftHanded = gameStatus.leftHanded;
				playerHeight = gameStatus.playerHeight;
				reduceDebris = gameStatus.reduceDebris;
				noHUD = gameStatus.noHUD;
				advancedHUD = gameStatus.advancedHUD;
				autoRestart = gameStatus.autoRestart;

				// notescores 配列サイズチェック (ノーツ数＋爆弾数以上か？)
				if (notesCount + bombsCount + obstacleEventCount > noteScores.Length)
				{
					Array.Resize(ref noteScores, notesCount + bombsCount + obstacleEventCount);
					Array.Resize(ref noteCutTime, notesCount + bombsCount + addArraySize);
					Array.Resize(ref cutSwingRating, notesCount + bombsCount + addArraySize);
				}
				// notescore 配列初期化
				while (initSize < noteScores.Length)
				{
					noteScores[initSize] = new NoteScore();
					initSize++;
				}
				noteScoresIdx = 0;
				noteCutTimeIdx = 0;
				if (gc_collect)
					GC.Collect();
			}
			else if (bs_event == finished_event_name || bs_event == failed_event_name)
			{
				end_time = Plugin.GetCurrentTime();
				end = true;
				cleared = bs_event;
				// Performance
				score = gameStatus.score;
				currentMaxScore = gameStatus.currentMaxScore;
				rank = gameStatus.rank;
				passedNotes = gameStatus.passedNotes;
				hitNotes = gameStatus.hitNotes;
				missedNotes = gameStatus.missedNotes;
				lastNoteScore = gameStatus.lastNoteScore;
				passedBombs = gameStatus.passedBombs;
				hitBombs = gameStatus.hitBombs;
				combo = gameStatus.combo;
				maxCombo = gameStatus.maxCombo;
				multiplier = gameStatus.multiplier;
				multiplierProgress = gameStatus.multiplierProgress;
				batteryEnergy = gameStatus.batteryEnergy;
			}
			if (bs_event == menu_event_name)
			{
				if (song)
				{
					int end_flag = 1;
					if (end == false)
					{
						end_flag = 0;
					}
					using (SQLiteConnection db_con = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
					{
						db_con.Open();
						SQLiteTransaction transaction = null;
						try
						{
							//MovieCutRecord
							using (SQLiteCommand db_cmd = new SQLiteCommand(db_con))
							{
								db_cmd.CommandText = "insert into MovieCutRecord(startTime, endTime, menuTime, cleared, endFlag, pauseCount, pluginVersion," +
													 "gameVersion, scene, mode, songName, songSubName, songAuthorName, levelAuthorName," +
													 "songHash, levelId, songBPM, noteJumpSpeed, songTimeOffset, start, paused, length, difficulty," + "" +
													 "notesCount, bombsCount, obstaclesCount, maxScore, maxRank," +
													 "environmentName, scorePercentage, score, currentMaxScore, rank, passedNotes, hitNotes," +
													 "missedNotes, lastNoteScore, passedBombs, hitBombs, combo, maxCombo, multiplier, obstacles," +
													 "instaFail, noFail, batteryEnergy, disappearingArrows, noBombs, songSpeed, songSpeedMultiplier," +
													 "noArrows, ghostNotes, failOnSaberClash, strictAngles, fastNotes, staticLights, leftHanded," +
													 "playerHeight, reduceDebris, noHUD, advancedHUD, autoRestart) values (" +
													 "@startTime, @endTime, @menuTime, @cleared, @endFlag, @pauseCount, @pluginVersion, @gameVersion, @scene, @mode," +
													 "@songName, @songSubName, @songAuthorName, @levelAuthorName, @songHash, @levelId, @songBPM, @noteJumpSpeed," +
													 "@songTimeOffset, @start, @paused, @length, @difficulty, @notesCount," +
													 "@bombsCount, @obstaclesCount, @maxScore, @maxRank, @environmentName, @scorePercentage, @score," +
													 "@currentMaxScore, @rank, @passedNotes, @hitNotes, @missedNotes, @lastNoteScore, @passedBombs," +
													 "@hitBombs, @combo, @maxCombo, @multiplier, @obstacles, @instaFail, @noFail, @batteryEnergy," +
													 "@disappearingArrows, @noBombs, @songSpeed, @songSpeedMultiplier, @noArrows, @ghostNotes," +
													 "@failOnSaberClash, @strictAngles, @fastNotes, @staticLights, @leftHanded, @playerHeight," +
													 "@reduceDebris, @noHUD, @advancedHUD, @autoRestart)";
								//独自カラム
								db_cmd.Parameters.Add(new SQLiteParameter("@startTime", start_time));
								db_cmd.Parameters.Add(new SQLiteParameter("@endTime", end_time));
								db_cmd.Parameters.Add(new SQLiteParameter("@menuTime", Plugin.GetCurrentTime()));
								db_cmd.Parameters.Add(new SQLiteParameter("@cleared", cleared));
								db_cmd.Parameters.Add(new SQLiteParameter("@endFlag", end_flag));
								db_cmd.Parameters.Add(new SQLiteParameter("@pauseCount", pause));
								//gameステータス
								db_cmd.Parameters.Add(new SQLiteParameter("@pluginVersion", Plugin.PluginVersion));
								db_cmd.Parameters.Add(new SQLiteParameter("@gameVersion", Plugin.GameVersion));
								db_cmd.Parameters.Add(new SQLiteParameter("@scene", scene));
								db_cmd.Parameters.Add(new SQLiteParameter("@mode", mode == null ? null : (partyMode ? "Party" : "Solo") + mode));
								//beatmapステータス
								db_cmd.Parameters.Add(new SQLiteParameter("@songName", songName));
								db_cmd.Parameters.Add(new SQLiteParameter("@songSubName", songSubName));
								db_cmd.Parameters.Add(new SQLiteParameter("@songAuthorName", songAuthorName));
								db_cmd.Parameters.Add(new SQLiteParameter("@levelAuthorName", levelAuthorName));
								db_cmd.Parameters.Add(new SQLiteParameter("@length", length));
								db_cmd.Parameters.Add(new SQLiteParameter("@songHash", songHash));
								db_cmd.Parameters.Add(new SQLiteParameter("@levelId", levelId));
								db_cmd.Parameters.Add(new SQLiteParameter("@songBPM", songBPM));
								db_cmd.Parameters.Add(new SQLiteParameter("@noteJumpSpeed", noteJumpSpeed));
								db_cmd.Parameters.Add(new SQLiteParameter("@songTimeOffset", songTimeOffset));
								if (start == 0)
									db_cmd.Parameters.Add(new SQLiteParameter("@start", null));
								else
									db_cmd.Parameters.Add(new SQLiteParameter("@start", start));
								if (paused == 0)
									db_cmd.Parameters.Add(new SQLiteParameter("@paused", null));
								else
									db_cmd.Parameters.Add(new SQLiteParameter("@paused", paused));
								db_cmd.Parameters.Add(new SQLiteParameter("@difficulty", difficulty));
								db_cmd.Parameters.Add(new SQLiteParameter("@notesCount", notesCount));
								db_cmd.Parameters.Add(new SQLiteParameter("@bombsCount", bombsCount));
								db_cmd.Parameters.Add(new SQLiteParameter("@obstaclesCount", obstaclesCount));
								db_cmd.Parameters.Add(new SQLiteParameter("@maxScore", maxScore));
								db_cmd.Parameters.Add(new SQLiteParameter("@maxRank", maxRank));
								db_cmd.Parameters.Add(new SQLiteParameter("@environmentName", environmentName));
								double scorePercentage;
								if (currentMaxScore == 0)
									scorePercentage = 0.0;
								else
									scorePercentage = double.Parse(String.Format("{0:F2}", ((double)score / (double)currentMaxScore) * 100.0));
								db_cmd.Parameters.Add(new SQLiteParameter("@scorePercentage", scorePercentage));
								//performanceステータス
								db_cmd.Parameters.Add(new SQLiteParameter("@score", score));
								db_cmd.Parameters.Add(new SQLiteParameter("@currentMaxScore", currentMaxScore));
								db_cmd.Parameters.Add(new SQLiteParameter("@rank", rank));
								db_cmd.Parameters.Add(new SQLiteParameter("@passedNotes", passedNotes));
								db_cmd.Parameters.Add(new SQLiteParameter("@hitNotes", hitNotes));
								db_cmd.Parameters.Add(new SQLiteParameter("@missedNotes", missedNotes));
								db_cmd.Parameters.Add(new SQLiteParameter("@lastNoteScore", lastNoteScore));
								db_cmd.Parameters.Add(new SQLiteParameter("@passedBombs", passedBombs));
								db_cmd.Parameters.Add(new SQLiteParameter("@hitBombs", hitBombs));
								db_cmd.Parameters.Add(new SQLiteParameter("@combo", combo));
								db_cmd.Parameters.Add(new SQLiteParameter("@maxCombo", maxCombo));
								//modステータス
								db_cmd.Parameters.Add(new SQLiteParameter("@multiplier", modifierMultiplier));
								if (modObstacles == null || modObstacles == "NoObstacles")
									db_cmd.Parameters.Add(new SQLiteParameter("@obstacles", false));
								else
									db_cmd.Parameters.Add(new SQLiteParameter("@obstacles", modObstacles));
								db_cmd.Parameters.Add(new SQLiteParameter("@instaFail", modInstaFail ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@noFail", modNoFail ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@batteryEnergy", modBatteryEnergy ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@disappearingArrows", modDisappearingArrows ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@noBombs", modNoBombs ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@songSpeed", modSongSpeed));
								db_cmd.Parameters.Add(new SQLiteParameter("@songSpeedMultiplier", songSpeedMultiplier));
								db_cmd.Parameters.Add(new SQLiteParameter("@noArrows", modNoArrows ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@ghostNotes", modGhostNotes ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@failOnSaberClash", modFailOnSaberClash ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@strictAngles", modStrictAngles ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@fastNotes", modFastNotes ? 1 : 0));
								//playerSettingsステータス
								db_cmd.Parameters.Add(new SQLiteParameter("@staticLights", staticLights ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@leftHanded", leftHanded ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@playerHeight", playerHeight));
								db_cmd.Parameters.Add(new SQLiteParameter("@reduceDebris", reduceDebris ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@noHUD", noHUD ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@advancedHUD", advancedHUD ? 1 : 0));
								db_cmd.Parameters.Add(new SQLiteParameter("@autoRestart", autoRestart ? 1 : 0));
								db_cmd.ExecuteNonQuery();

								if (db_notes_score)
								{
									//NoteScore
									// トランザクションを開始します。
									transaction = db_con.BeginTransaction();
									db_cmd.CommandText = "insert into NoteScore(time, cutTime, startTime, event, score, currentMaxScore, rank, " +
														 "passedNotes, hitNotes, missedNotes, lastNoteScore, passedBombs, " +
														 "hitBombs, combo, maxCombo, multiplier, multiplierProgress, " +
														 "batteryEnergy, noteID, noteType, noteCutDirection, noteLine, " +
														 "noteLayer, speedOK, directionOK, saberTypeOK, wasCutTooSoon, " +
														 "initialScore, beforeScore, afterScore, cutDistanceScore, finalScore, cutMultiplier, " +
														 "saberSpeed, saberDirX, saberDirY, saberDirZ, saberType, " +
														 "swingRating, swingRatingFullyCut, timeDeviation, cutDirectionDeviation, cutPointX, " +
														 "cutPointY, cutPointZ, cutNormalX, cutNormalY, cutNormalZ, cutDistanceToCenter, " +
														 "timeToNextBasicNote) values (" +
														 "@time, @cutTime, @startTime, @event, @score, @currentMaxScore, @rank, @passedNotes, " +
														 "@hitNotes, @missedNotes, @lastNoteScore, @passedBombs, @hitBombs, @combo, " +
														 "@maxCombo, @multiplier, @multiplierProgress, @batteryEnergy, @noteID, @noteType, " +
														 "@noteCutDirection, @noteLine, @noteLayer, @speedOK, @directionOK, @saberTypeOK, " +
														 "@wasCutTooSoon, @initialScore, @beforeScore, @afterScore, @cutDistanceScore, @finalScore, @cutMultiplier, " +
														 "@saberSpeed, @saberDirX, @saberDirY, @saberDirZ, @saberType, @swingRating, @swingRatingFullyCut, " +
														 "@timeDeviation, @cutDirectionDeviation, @cutPointX, @cutPointY, @cutPointZ, @cutNormalX, " +
														 "@cutNormalY, @cutNormalZ, @cutDistanceToCenter, @timeToNextBasicNote)";
									for (int idx = 0; idx < noteScoresIdx; idx++)
									{
										//Plugin.log.Debug(idx.ToString() + ":" + noteScores[idx].time.ToString()); //デバッグ用
										db_cmd.Parameters.Add(new SQLiteParameter("@time", noteScores[idx].time));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutTime", noteScores[idx].cutTime));
										db_cmd.Parameters.Add(new SQLiteParameter("@startTime", start_time));
										db_cmd.Parameters.Add(new SQLiteParameter("@event", noteScores[idx].bs_event));
										db_cmd.Parameters.Add(new SQLiteParameter("@score", noteScores[idx].score));
										db_cmd.Parameters.Add(new SQLiteParameter("@currentMaxScore", noteScores[idx].currentMaxScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@rank", noteScores[idx].rank));
										db_cmd.Parameters.Add(new SQLiteParameter("@passedNotes", noteScores[idx].passedNotes));
										db_cmd.Parameters.Add(new SQLiteParameter("@hitNotes", noteScores[idx].hitNotes));
										db_cmd.Parameters.Add(new SQLiteParameter("@missedNotes", noteScores[idx].missedNotes));
										db_cmd.Parameters.Add(new SQLiteParameter("@lastNoteScore", noteScores[idx].lastNoteScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@passedBombs", noteScores[idx].passedBombs));
										db_cmd.Parameters.Add(new SQLiteParameter("@hitBombs", noteScores[idx].hitBombs));
										db_cmd.Parameters.Add(new SQLiteParameter("@combo", noteScores[idx].combo));
										db_cmd.Parameters.Add(new SQLiteParameter("@maxCombo", noteScores[idx].maxCombo));
										db_cmd.Parameters.Add(new SQLiteParameter("@multiplier", noteScores[idx].multiplier));
										db_cmd.Parameters.Add(new SQLiteParameter("@multiplierProgress", noteScores[idx].multiplierProgress));
										db_cmd.Parameters.Add(new SQLiteParameter("@batteryEnergy", noteScores[idx].batteryEnergy));
										db_cmd.Parameters.Add(new SQLiteParameter("@noteID", noteScores[idx].noteID));
										db_cmd.Parameters.Add(new SQLiteParameter("@noteType", noteScores[idx].noteType));
										db_cmd.Parameters.Add(new SQLiteParameter("@noteCutDirection", noteScores[idx].noteCutDirection));
										db_cmd.Parameters.Add(new SQLiteParameter("@noteLine", noteScores[idx].noteLine));
										db_cmd.Parameters.Add(new SQLiteParameter("@noteLayer", noteScores[idx].noteLayer));
										db_cmd.Parameters.Add(new SQLiteParameter("@speedOK", noteScores[idx].speedOK == true ? 1 : 0));
										db_cmd.Parameters.Add(new SQLiteParameter("@directionOK", noteScores[idx].directionOK == null ? null : (noteScores[idx].directionOK == true ? (int?)1 : (int?)0)));
										db_cmd.Parameters.Add(new SQLiteParameter("@saberTypeOK", noteScores[idx].saberTypeOK == null ? null : (noteScores[idx].saberTypeOK == true ? (int?)1 : (int?)0)));
										db_cmd.Parameters.Add(new SQLiteParameter("@wasCutTooSoon", noteScores[idx].wasCutTooSoon == true ? 1 : 0));
										db_cmd.Parameters.Add(new SQLiteParameter("@initialScore", noteScores[idx].initialScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@beforeScore", noteScores[idx].beforeScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@afterScore", noteScores[idx].afterScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutDistanceScore", noteScores[idx].cutDistanceScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@finalScore", noteScores[idx].finalScore));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutMultiplier", noteScores[idx].cutMultiplier));
										db_cmd.Parameters.Add(new SQLiteParameter("@saberSpeed", noteScores[idx].saberSpeed));
										db_cmd.Parameters.Add(new SQLiteParameter("@saberDirX", noteScores[idx].saberDirX));
										db_cmd.Parameters.Add(new SQLiteParameter("@saberDirY", noteScores[idx].saberDirY));
										db_cmd.Parameters.Add(new SQLiteParameter("@saberDirZ", noteScores[idx].saberDirZ));
										db_cmd.Parameters.Add(new SQLiteParameter("@saberType", noteScores[idx].saberType));
										db_cmd.Parameters.Add(new SQLiteParameter("@swingRating", noteScores[idx].swingRating));
										db_cmd.Parameters.Add(new SQLiteParameter("@swingRatingFullyCut", noteScores[idx].swingRatingFullyCut));
										db_cmd.Parameters.Add(new SQLiteParameter("@timeDeviation", noteScores[idx].timeDeviation));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutDirectionDeviation", noteScores[idx].cutDirectionDeviation));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutPointX", noteScores[idx].cutPointX));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutPointY", noteScores[idx].cutPointY));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutPointZ", noteScores[idx].cutPointZ));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutNormalX", noteScores[idx].cutNormalX));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutNormalY", noteScores[idx].cutNormalY));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutNormalZ", noteScores[idx].cutNormalZ));
										db_cmd.Parameters.Add(new SQLiteParameter("@cutDistanceToCenter", noteScores[idx].cutDistanceToCenter));
										db_cmd.Parameters.Add(new SQLiteParameter("@timeToNextBasicNote", noteScores[idx].timeToNextBasicNote));
										var result = db_cmd.ExecuteNonQuery();
										// データ更新できない場合
										if (result != 1)
										{
											Plugin.log.Error("DB NoteScore INSERT Error");
											transaction.Rollback();
											transaction = null;
											break;
										}
									}
									if (transaction != null)
										transaction.Commit();
								}
							}
						}
						// 例外が発生した場合
						catch (Exception e)
						{
							// トランザクションが有効な場合
							if (transaction != null)
							{
								Plugin.log.Error("DB NoteScore INSERT Error " + e.Message);
								transaction.Rollback();
							}
						}
						finally
						{
							db_con.Close();
						}
					}
				}
				song = false;
				end = false;
				pause = 0;
				cleared = "";
				if (gc_collect)
					GC.Collect();
			}
			if (bs_event == resume_event_name || bs_event == pause_event_name)
			{
				cleared = bs_event;
				using (SQLiteConnection db_con = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
				{
					db_con.Open();
					try
					{
						using (SQLiteCommand db_cmd = new SQLiteCommand(db_con))
						{
							db_cmd.CommandText = "insert into MovieCutPause(time, event) values (@time, @event)";
							db_cmd.Parameters.Add(new SQLiteParameter("@time", Plugin.GetCurrentTime()));
							db_cmd.Parameters.Add(new SQLiteParameter("@event", bs_event));
							db_cmd.ExecuteNonQuery();
						}
					}
					finally
					{
						db_con.Close();
					}
				}
				if (bs_event == resume_event_name)
				{
					start = gameStatus.start;
				}
				if (bs_event == pause_event_name)
				{
					++pause;
					// Performance
					paused = gameStatus.paused;
					score = gameStatus.score;
					currentMaxScore = gameStatus.currentMaxScore;
					rank = gameStatus.rank;
					passedNotes = gameStatus.passedNotes;
					hitNotes = gameStatus.hitNotes;
					missedNotes = gameStatus.missedNotes;
					lastNoteScore = gameStatus.lastNoteScore;
					passedBombs = gameStatus.passedBombs;
					hitBombs = gameStatus.hitBombs;
					combo = gameStatus.combo;
					maxCombo = gameStatus.maxCombo;
					multiplier = gameStatus.multiplier;
					multiplierProgress = gameStatus.multiplierProgress;
					batteryEnergy = gameStatus.batteryEnergy;
					end_time = Plugin.GetCurrentTime();
				}
			}
			if (bs_event == noteCut_event_name && db_notes_score)
			{
				noteCutTimeIdx = gameStatus.noteID + 1;
				// noteCutTime 配列サイズチェック
				if (noteCutTimeIdx + 1 >= noteCutTime.Length)
				{
					Array.Resize(ref noteCutTime, noteCutTimeIdx + addArraySize);
					Array.Resize(ref cutSwingRating, noteCutTimeIdx + addArraySize);
				}
				noteCutTime[noteCutTimeIdx] = Plugin.GetCurrentTime();
				cutSwingRating[noteCutTimeIdx] = gameStatus.swingRating;
			}
			if ((bs_event == noteFullyCut_event_name || bs_event == noteMissed_event_name || bs_event == bombCut_event_name || bs_event == bombMissed_event_name || bs_event == obstacleEnter_event_name || bs_event == obstacleExit_event_name) && db_notes_score)
			{
				// notescores 配列サイズチェック
				if (noteScoresIdx + 1 >= noteScores.Length)
					Array.Resize(ref noteScores, noteScoresIdx + addArraySize);
				// notescore 配列初期化
				while (initSize < noteScores.Length)
				{
					noteScores[initSize] = new NoteScore();
					initSize++;
				}

				noteScores[noteScoresIdx].time = Plugin.GetCurrentTime();
				if (bs_event == noteFullyCut_event_name)
				{
					noteScores[noteScoresIdx].cutTime = noteCutTime[gameStatus.noteID + 1];
				}
				else
				{
					noteScores[noteScoresIdx].cutTime = null;
				}
				noteScores[noteScoresIdx].bs_event = bs_event;
				noteScores[noteScoresIdx].score = gameStatus.score;
				noteScores[noteScoresIdx].currentMaxScore = gameStatus.currentMaxScore;
				noteScores[noteScoresIdx].rank = gameStatus.rank;
				noteScores[noteScoresIdx].passedNotes = gameStatus.passedNotes;
				noteScores[noteScoresIdx].hitNotes = gameStatus.hitNotes;
				noteScores[noteScoresIdx].missedNotes = gameStatus.missedNotes;
				noteScores[noteScoresIdx].lastNoteScore = gameStatus.lastNoteScore;
				noteScores[noteScoresIdx].passedBombs = gameStatus.passedBombs;
				noteScores[noteScoresIdx].hitBombs = gameStatus.hitBombs;
				noteScores[noteScoresIdx].combo = gameStatus.combo;
				noteScores[noteScoresIdx].maxCombo = gameStatus.maxCombo;
				noteScores[noteScoresIdx].multiplier = gameStatus.multiplier;
				noteScores[noteScoresIdx].multiplierProgress = gameStatus.multiplierProgress;
				noteScores[noteScoresIdx].batteryEnergy = gameStatus.batteryEnergy;
				if (bs_event == noteFullyCut_event_name || bs_event == bombCut_event_name || bs_event == noteMissed_event_name || bs_event == bombMissed_event_name)
				{
					noteScores[noteScoresIdx].noteID = gameStatus.noteID;
					noteScores[noteScoresIdx].noteType = gameStatus.noteType;
					noteScores[noteScoresIdx].noteCutDirection = gameStatus.noteCutDirection;
					noteScores[noteScoresIdx].noteLine = gameStatus.noteLine;
					noteScores[noteScoresIdx].noteLayer = gameStatus.noteLayer;
					noteScores[noteScoresIdx].speedOK = gameStatus.speedOK;
					noteScores[noteScoresIdx].directionOK = gameStatus.directionOK;
					noteScores[noteScoresIdx].saberTypeOK = gameStatus.saberTypeOK;
					noteScores[noteScoresIdx].wasCutTooSoon = gameStatus.wasCutTooSoon;
					noteScores[noteScoresIdx].initialScore = gameStatus.initialScore;
					noteScores[noteScoresIdx].beforeScore = gameStatus.initialScore - gameStatus.cutDistanceScore;
					noteScores[noteScoresIdx].afterScore = gameStatus.finalScore - gameStatus.initialScore;
					noteScores[noteScoresIdx].cutDistanceScore = gameStatus.cutDistanceScore;
					noteScores[noteScoresIdx].finalScore = gameStatus.finalScore;
					noteScores[noteScoresIdx].cutMultiplier = gameStatus.cutMultiplier;
					noteScores[noteScoresIdx].saberSpeed = gameStatus.saberSpeed;
					noteScores[noteScoresIdx].saberDirX = gameStatus.saberDirX;
					noteScores[noteScoresIdx].saberDirY = gameStatus.saberDirY;
					noteScores[noteScoresIdx].saberDirZ = gameStatus.saberDirZ;
					noteScores[noteScoresIdx].saberType = gameStatus.saberType;
					noteScores[noteScoresIdx].swingRating = cutSwingRating[gameStatus.noteID + 1];
					if (bs_event == noteFullyCut_event_name || bs_event == bombCut_event_name)
						noteScores[noteScoresIdx].swingRatingFullyCut = gameStatus.swingRating;
					else
						noteScores[noteScoresIdx].swingRatingFullyCut = 0;
					noteScores[noteScoresIdx].timeDeviation = gameStatus.timeDeviation;
					noteScores[noteScoresIdx].cutDirectionDeviation = gameStatus.cutDirectionDeviation;
					noteScores[noteScoresIdx].cutPointX = gameStatus.cutPointX;
					noteScores[noteScoresIdx].cutPointY = gameStatus.cutPointY;
					noteScores[noteScoresIdx].cutPointZ = gameStatus.cutPointZ;
					noteScores[noteScoresIdx].cutNormalX = gameStatus.cutNormalX;
					noteScores[noteScoresIdx].cutNormalY = gameStatus.cutNormalY;
					noteScores[noteScoresIdx].cutNormalZ = gameStatus.cutNormalZ;
					noteScores[noteScoresIdx].cutDistanceToCenter = gameStatus.cutDistanceToCenter;
					noteScores[noteScoresIdx].timeToNextBasicNote = gameStatus.timeToNextBasicNote;
				}
				else
				{
					noteScores[noteScoresIdx].noteID = null;
					noteScores[noteScoresIdx].noteType = null;
					noteScores[noteScoresIdx].noteCutDirection = null;
					noteScores[noteScoresIdx].noteLine = null;
					noteScores[noteScoresIdx].noteLayer = null;
					noteScores[noteScoresIdx].speedOK = null;
					noteScores[noteScoresIdx].directionOK = null;
					noteScores[noteScoresIdx].saberTypeOK = null;
					noteScores[noteScoresIdx].wasCutTooSoon = null;
					noteScores[noteScoresIdx].initialScore = null;
					noteScores[noteScoresIdx].afterScore = null;
					noteScores[noteScoresIdx].cutDistanceScore = null;
					noteScores[noteScoresIdx].finalScore = null;
					noteScores[noteScoresIdx].cutMultiplier = null;
					noteScores[noteScoresIdx].saberSpeed = null;
					noteScores[noteScoresIdx].saberDirX = null;
					noteScores[noteScoresIdx].saberDirY = null;
					noteScores[noteScoresIdx].saberDirZ = null;
					noteScores[noteScoresIdx].saberType = null;
					noteScores[noteScoresIdx].swingRating = null;
					noteScores[noteScoresIdx].swingRatingFullyCut = null;
					noteScores[noteScoresIdx].timeDeviation = null;
					noteScores[noteScoresIdx].cutDirectionDeviation = null;
					noteScores[noteScoresIdx].cutPointX = null;
					noteScores[noteScoresIdx].cutPointY = null;
					noteScores[noteScoresIdx].cutPointZ = null;
					noteScores[noteScoresIdx].cutNormalX = null;
					noteScores[noteScoresIdx].cutNormalY = null;
					noteScores[noteScoresIdx].cutNormalZ = null;
					noteScores[noteScoresIdx].cutDistanceToCenter = null;
					noteScores[noteScoresIdx].timeToNextBasicNote = null;
				}
				noteScoresIdx++;
			}
		}
		public void DbCheck()
		{
			//設定ファイルのチェック＆読み込み
			if (File.Exists(settingFile))
			{
				using (StreamReader reader = new StreamReader(settingFile, encUTF8))
				{
					string str = reader.ReadToEnd();
					reader.Close();
					JSONNode json_read = JSON.Parse(str);
					dbFile = (json_read["dbfile"].Value ?? defaultDbFile);
					if (dbFile.Trim() == "")
						dbFile = defaultDbFile;
					else if (!Directory.Exists(System.IO.Path.GetDirectoryName(dbFile)))
						dbFile = defaultDbFile;
					if (json_read["http_scenechange"].Value.Trim() != "")
						http_scenechange = json_read["http_scenechange"];
					if (json_read["http_scorechanged"].Value.Trim() != "")
						http_scorechanged = json_read["http_scorechanged"];
					if (json_read["http_notecut"].Value.Trim() != "")
						http_notecut = json_read["http_notecut"];
					if (json_read["http_notefullycut"].Value.Trim() != "")
						http_notefullycut = json_read["http_notefullycut"];
					if (json_read["http_notemissed"].Value.Trim() != "")
						http_notemissed = json_read["http_notemissed"];
					if (json_read["http_bombcut"].Value.Trim() != "")
						http_bombcut = json_read["http_bombcut"];
					if (json_read["http_bombmissed"].Value.Trim() != "")
						http_bombmissed = json_read["http_bombmissed"];
					if (json_read["http_beatmapevent"].Value.Trim() != "")
						http_beatmapevent = json_read["http_beatmapevent"];
					if (json_read["http_obstacle"].Value.Trim() != "")
						http_obstacle = json_read["http_obstacle"];
					if (json_read["db_notes_score"].Value.Trim() != "")
						db_notes_score = json_read["db_notes_score"];
					if (json_read["gc_collect"].Value.Trim() != "")
						gc_collect = json_read["gc_collect"];
				}
			}
			else
			{
				//設定ファイルがない場合、作成
				dbFile = defaultDbFile;
				JSONObject json_write = new JSONObject();
				json_write["dbfile"] = null;
				json_write["http_scenechange"] = http_scenechange;
				json_write["http_scorechanged"] = http_scorechanged;
				json_write["http_notecut"] = http_notecut;
				json_write["http_notefullycut"] = http_notefullycut;
				json_write["http_notemissed"] = http_notemissed;
				json_write["http_bombcut"] = http_bombcut;
				json_write["http_bombmissed"] = http_bombmissed;
				json_write["http_beatmapevent"] = http_beatmapevent;
				json_write["http_obstacle"] = http_obstacle;
				json_write["db_notes_score"] = db_notes_score;
				json_write["gc_collect"] = gc_collect;
				using (StreamWriter writer = new StreamWriter(settingFile, false, encUTF8))
				{
					writer.WriteLine(json_write.ToString(4));
					writer.Close();
				}
			}
			using (SQLiteConnection db_con = new SQLiteConnection("Data Source=" + dbFile + ";Version=3;"))
			{
				db_con.Open();
				try
				{
					using (SQLiteCommand db_cmd = new SQLiteCommand(db_con))
					{
						//テーブル作成
						db_cmd.CommandText = "CREATE TABLE IF NOT EXISTS MovieCutRecord(" +
							"startTime INTEGER NOT NULL PRIMARY KEY," +
							"endTime INTEGER," +
							"menuTime INTEGER NOT NULL," +
							"cleared TEXT," +
							"endFlag INTEGER NOT NULL," +
							"pauseCount INTEGER NOT NULL," +
							"pluginVersion TEXT," +
							"gameVersion TEXT," +
							"scene TEXT," +
							"mode TEXT," +
							"songName TEXT," +
							"songSubName TEXT," +
							"songAuthorName TEXT," +
							"levelAuthorName TEXT," +
							"songHash TEXT," +
							"levelId TEXT," +
							"songBPM REAL," +
							"noteJumpSpeed REAL," +
							"songTimeOffset INTEGER," +
							"start TEXT," +
							"paused TEXT," +
							"length INTEGER," +
							"difficulty TEXT," +
							"notesCount INTEGER," +
							"bombsCount INTEGER," +
							"obstaclesCount INTEGER," +
							"maxScore INTEGER," +
							"maxRank TEXT," +
							"environmentName TEXT," +
							"scorePercentage REAL," +
							"score INTEGER," +
							"currentMaxScore INTEGER," +
							"rank TEXT," +
							"passedNotes INTEGER," +
							"hitNotes INTEGER," +
							"missedNotes INTEGER," +
							"lastNoteScore INTEGER," +
							"passedBombs INTEGER," +
							"hitBombs INTEGER," +
							"combo INTEGER," +
							"maxCombo INTEGER," +
							"multiplier REAL," +
							"obstacles TEXT," +
							"instaFail INTEGER," +
							"noFail INTEGER," +
							"batteryEnergy INTEGER," +
							"disappearingArrows INTEGER," +
							"noBombs INTEGER," +
							"songSpeed TEXT," +
							"songSpeedMultiplier REAL," +
							"noArrows INTEGER," +
							"ghostNotes INTEGER," +
							"failOnSaberClash INTEGER," +
							"strictAngles INTEGER," +
							"fastNotes INTEGER," +
							"staticLights INTEGER," +
							"leftHanded INTEGER," +
							"playerHeight REAL," +
							"reduceDebris INTEGER," +
							"noHUD INTEGER," +
							"advancedHUD INTEGER," +
							"autoRestart INTEGER);";
						db_cmd.ExecuteNonQuery();
						db_cmd.CommandText = "CREATE TABLE IF NOT EXISTS MovieCutPause(" +
							"time INTEGER NOT NULL PRIMARY KEY," +
							"event TEXT);";
						db_cmd.ExecuteNonQuery();
						db_cmd.CommandText = "CREATE TABLE IF NOT EXISTS NoteScore(" +
							"time INTEGER," +
							"cutTime INTEGER," +
							"startTime INTEGER," +
							"event TEXT," +
							"score INTEGER," +
							"currentMaxScore INTEGER," +
							"rank TEXT," +
							"passedNotes INTEGER," +
							"hitNotes INTEGER," +
							"missedNotes INTEGER," +
							"lastNoteScore INTEGER," +
							"passedBombs INTEGER," +
							"hitBombs INTEGER," +
							"combo INTEGER," +
							"maxCombo INTEGER," +
							"multiplier INTEGER," +
							"multiplierProgress REAL," +
							"batteryEnergy INTEGER," +
							"noteID INTEGER," +
							"noteType TEXT," +
							"noteCutDirection TEXT," +
							"noteLine INTEGER," +
							"noteLayer INTEGER," +
							"speedOK INTEGER," +
							"directionOK INTEGER," +
							"saberTypeOK INTEGER," +
							"wasCutTooSoon INTEGER," +
							"initialScore INTEGER," +
							"beforeScore INTEGER," +
							"afterScore INTEGER," +
							"cutDistanceScore INTEGER," +
							"finalScore INTEGER," +
							"cutMultiplier INTEGER," +
							"saberSpeed REAL," +
							"saberDirX REAL," +
							"saberDirY REAL," +
							"saberDirZ REAL," +
							"saberType TEXT," +
							"swingRating REAL," +
							"swingRatingFullyCut REAL," +
							"timeDeviation REAL," +
							"cutDirectionDeviation REAL," +
							"cutPointX REAL," +
							"cutPointY REAL," +
							"cutPointZ REAL," +
							"cutNormalX REAL," +
							"cutNormalY REAL," +
							"cutNormalZ REAL," +
							"cutDistanceToCenter REAL," +
							"timeToNextBasicNote REAL);";
						db_cmd.ExecuteNonQuery();
						DbColumnCheck(db_cmd, "MovieCutRecord", "levelId", "TEXT");
						DbColumnCheck(db_cmd, "NoteScore", "beforeScore", "INTEGER");
					}
				}
				finally
				{
					db_con.Close();
				}
			}
		}
		public void DbColumnCheck(SQLiteCommand db_cmd, string table, string column, string type)
		{
			db_cmd.CommandText = "PRAGMA table_info('" + table + "');";
			bool column_check = true;
			using (SQLiteDataReader db_reader = db_cmd.ExecuteReader())
			{
				while (db_reader.Read())
				{
					if (column == (string)db_reader["name"])
					{
						column_check = false;
						break;
					}
				}
			}
			if (column_check)
			{
				db_cmd.CommandText = "ALTER TABLE " + table + " ADD COLUMN " + column + " " + type + ";";
				db_cmd.ExecuteNonQuery();
			}
		}
		public bool EventSendCheck(string event_name)
        {
			switch(event_name)
            {
				case songStart_event_name:
				case finished_event_name:
				case failed_event_name:
				case menu_event_name:
				case pause_event_name:
				case resume_event_name:
					return http_scenechange;
				case scoreChanged_event_name:
					return http_scorechanged;
				case noteCut_event_name:
					return http_notecut;
				case noteFullyCut_event_name:
					return http_notefullycut;
				case noteMissed_event_name:
					return http_notemissed;
				case bombCut_event_name:
					return http_bombcut;
				case bombMissed_event_name:
					return http_bombmissed;
				case beatmapEvent_event_name:
					return http_beatmapevent;
				case obstacleEnter_event_name:
				case obstacleExit_event_name:
					return http_obstacle;
				default:
					return true;
			}
        }
	}
}
