BeatSaber1.13.0対応までで本ツールは開発終了します

BeatSaber1.13.2以降は [DataRecorder](https://github.com/rynan4818/DataRecorder)を使用して下さい
---
**The development of this tool will be finished by BeatSaber1.13.0.**

Please use [DataRecorder](https://github.com/rynan4818/DataRecorder) for BeatSaber1.13.2 or later.

---
# Beat Saber HTTP Status +Database
---
このプラグインは、opl氏が製作の[Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status)の機能に、SQLite ver3 形式でのデータベース記録機能を追加したものです。

拙作のBeat Saber プレイ動画カットツール（[BS Movie Cut](https://github.com/rynan4818/bs-movie-cut)）で使用するために製作しました。

HTTP Statusの機能は同じなので、置き換えて使用することを想定しています。

---

This plug-in adds the database recording function in SQLite ver3 format to the function of [Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status) created by opl.

I created this for use in my Beat Saber play movie cut tool([BS Movie Cut](https://github.com/rynan4818/bs-movie-cut)).

Since the function of HTTP Status is the same, it is assumed to be used as a replacement for this tool.

## インストール方法 (How to install)

1. [Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status)のインストール手順によって、HTTP Statusをインストールします。そして、HTTP Statusが正しく動作するか一度確認して下さい。

2. 次に[リリースページ](https://github.com/rynan4818/beatsaber-http-status-db/releases)から最新のリリースをダウンロードします。

3. zipをBeat Saberフォルダに解凍します。(`SQLite.Interop.dll` と `System.Data.SQLite.dll` を `Beat Saber\Beat Saber_Data\Managed` フォルダにコピーし、`Beat Saber\Plugins` フォルダの `BeatSaberHTTPStatus.dll` を、本リリースのファイルと差し替えて下さい。)

4. 一度Beat Saberを起動すると、`UserData` フォルダに下記内容の `movie_cut_record.json` ファイルが作成されます。
必要に応じて設定値を変更して下さい。[BS Movie Cut](https://github.com/rynan4818/bs-movie-cut)を使用すれば、GUI画面で設定可能です。

5. ModAssistant を使用されている場合は、オリジナルのHTTP Statusに更新されてしまうため HTTP Status のチェックを外しておいて下さい。
---
1. Follow the [Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status) installation instructions to install HTTP Status.
Verify that HTTP Status is working properly.

2. You can then download the latest release from the [release page](https://github.com/rynan4818/beatsaber-http-status-db/releases).

3. Extract the zip to your Beat Saber folder. (`SQLite.Interop.dll` and `System.Data.SQLite.dll` to Copy to the `Beat Saber\Beat Saber_Data\Managed` folder. `BeatSaberHTTPStatus.dll` to `Beat Saber\Plugins` overwrite the same file in the folder.)

4. Once you start the Beat Saber, you will see a `movie_cut_record.json` in the `UserData` folder with the following content file is created. Change the settings as needed. If you use [BS Movie Cut](https://github.com/rynan4818/bs-movie-cut), it is possible to set up on GUI screen.

5. If you are using ModAssistant, leave HTTP Status unchecked as it will be updated to the original HTTP Status.

```json
{
    "dbfile" : null,
    "http_scenechange" : true,
    "http_scorechanged" : true,
    "http_notecut" : true,
    "http_notefullycut" : true,
    "http_notemissed" : true,
    "http_bombcut" : true,
    "http_bombmissed" : true,
    "http_beatmapevent" : true,
    "http_obstacle" : true,
    "db_notes_score" : true,
    "gc_collect" : true
}
```
### movie_cut_record.json ファイルの各項目の説明 (Description of each item in the file)
- dbfile ・・・ 記録するデータベースファイルの保存場所を変更する場合に指定します。nullの場合は`UserData`フォルダに`beatsaber.db`のファイル名で記録します。変更する場合は、データベースファイルをフルパスで記載して下さい。パスの`\`マークは`\\`と記載して下さい。

以下の項目は true か false で指定して下さい。
- db_notes_score ・・・ trueの時にノーツ毎のスコア(Note cut object)の記録を行います。
- gc_collect ・・・ trueの時にプレイの最初(songStart時)と最後(menuに戻った時)にガベージコレクション(GC.Collect)処理を行います。

以下の項目はHTTP StatusのWebSocketで送信するイベントのon/offが設定可能です。
- http_scenechange ・・・ trueの時に hello, songStart, finished, failed, menu, pause, resume イベントを送信します。
- http_scorechanged ・・・ trueの時に scoreChanged イベントを送信します。
- http_notecut ・・・ trueの時に noteCut イベントを送信します。
- http_notefullycut ・・・ trueの時に noteFullyCut イベントを送信します。
- http_notemissed ・・・ trueの時に noteMissed イベントを送信します。
- http_bombcut ・・・ trueの時に bombCut イベントを送信します。
- http_bombmissed ・・・ trueの時に bombMissed イベントを送信します。
- http_beatmapevent ・・・ trueの時に beatmapEvent イベントを送信します。
- http_obstacle ・・・ trueの時に obstacleEnter, obstacleExit イベントを送信します。

---
- dbfile ・・・ Specify if you want to change the location of the database file to be recorded.
If null, it will be recorded in the `UserData` folder with the file name `beatsaber.db`.
If you change it, describe the full path to the database file.
The `\` mark on the path should be written as `\\`.

Specify the following items as true or false.
- db_notes_score ・・・ When true, the score for each note (Note cut object) is recorded.
- gc_collect ・・・ When true, at the beginning (at songStart) and at the end (when you return to the menu) Garbage Collection (GC.Collect) processing.

The following items allow you to turn on/off events sent by HTTP Status WebSocket.
- http_scenechange ・・・ If true, send `hello, songStart, finished, finished, failed, menu, pause, resume` event.
- http_scorechanged ・・・ If it is true, it sends `scoreChanged` event.
- http_notecut ・・・ If it is true, it sends `noteCut` event.
- http_notefullycut ・・・ If it is true, it sends `noteFullyCut` event.
- http_notemissed ・・・ If it is true, `noteMissed` event is sent.
- http_bombcut ・・・ If it is true, `bombCut` event is sent.
- http_bombmissed ・・・ If it is true, `bombMissed` event is sent.
- http_beatmapevent ・・・ If it is true, `beatmapEvent` event is sent.
- http_obstacle ・・・ If true, send `obstacleEnter, obstacleExit` event.

### 設定例 (Example settings)

以下は[Beat Saber Overlay no score](https://github.com/rynan4818/beat-saber-overlay/tree/noscore_master)（Beat Saber Overlayのスコア表示を消したVer）を使用し、記録するデータベースファイルの保存先を変更した設定例です。

(The following is an example of how to use the [Beat Saber Overlay no score](https://github.com/rynan4818/beat-saber-overlay/tree/noscore_master)(This is the version with no score display in Beat Saber Overlay) to change the destination of the database files to be recorded.)

```json
{
    "dbfile" : "C:\\TOOL\\bs_movie_cut\\beatsaber.db",
    "http_scenechange" : true,
    "http_scorechanged" : false,
    "http_notecut" : false,
    "http_notefullycut" : true,
    "http_notemissed" : false,
    "http_bombcut" : false,
    "http_bombmissed" : false,
    "http_beatmapevent" : false,
    "http_obstacle" : false,
    "db_notes_score" : true,
    "gc_collect" : true
}
```

## 開発者向け (Developers)

### HTTP Statusの使用方法 (Using HTTP Status)

HTTP Statusのプロコトルのドキュメントは[protocol.md](protocol.md)にあります。 HTTP Statusとしての機能はオリジナルから変更はありません。

(Protocol documentation can be found in [protocol.md](protocol.md). The HTTP Status function is unchanged from the original.)

### データベースの使用方法 (Using Database)

データベースファイルは `MovieCutRecord` テーブルにHTTP Statusの `Status object` の内容が記録され、`NoteScore` テーブルに `Note cut object` の内容が記録されます。各カラム名はHTTP Statusプロコトルの各項目名と基本的に一致しています。但し、一部項目名の衝突があるため名称を変更していますが、見れば判るカラム名にしてあります。

(The database files are stored in the `MovieCutRecord` table with the contents of the HTTP Status `Status object` and the `NoteScore` table with the contents of the `Note cut object`. Each column name is basically the same as an item name in the HTTP Status protocol. However, the names have been changed to reflect the conflicting names of some of the fields, but the names have been changed to make them recognizable.)

### 本ツールへの貢献 (Contributing to HTTP Status +Database)

初心者向けのビルド方法の[ガイド(日本語)](https://drive.google.com/open?id=1g2XpANHnplUtqvxoTm_-htHh2joN-qFQDTtr0Aez2Y4)を作成しました。

pull request をする前に、[投稿ガイド](CONTRIBUTING.md)を読んで下さい。

このプロジェクトをビルドするには、 `BeatSaberHTTPStatus/BeatSaberHTTPStatusPlugin.csproj.user` ファイルを作成し、Beat Saberがインストールされた場所を指定する必要があります。

---

We have created a [guide(Japanese)](https://drive.google.com/open?id=1g2XpANHnplUtqvxoTm_-htHh2joN-qFQDTtr0Aez2Y4) for beginners on how to build.

Before opening a pull request, please read the [contributing guide](CONTRIBUTING.md).

To build this project you will need to create a `BeatSaberHTTPStatus/BeatSaberHTTPStatusPlugin.csproj.user` file specifying where the game is located on your disk:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- Change this path if necessary. Make sure it ends with a backslash. -->
    <GameDirPath>C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\</GameDirPath>
  </PropertyGroup>
</Project>
```

または、`libs/beatsaber` フォルダにBeatSaberのインストールフォルダ構造を作成して、使用するDLLファイルを配置することもできます。その際に必要なDLLファイルは[プロジェクトファイル](BeatSaberHTTPStatus/BeatSaberHTTPStatusPlugin.csproj)を参照して下さい。

以下のプロパティは `.csproj.user` ファイルかコマンドライン (`/p:<name>=<value>`) で指定することができます。
(The following properties can be specified either in the `.csproj.user` file or through the command line (`/p:<name>=<value>`):)

- `GameDirPath`: ビートセイバーディレクトリを指すバックスラッシュで終わるパス。必要なゲームDLLを探すために使用します。
((`GameDirPath`: Path ending with a backslash pointing to the Beat Saber directory. Used to locate required game DLLs.)

- `OutputZip` = `true`/`false`: .zipファイルの生成を有効にするかどうかを指定します。これは `Debug` 設定用のzipを取得するために使用することができます。
(`OutputZip` = `true`/`false`: Enable/disable generating the .zip file. Can be used to get a zip for the `Debug` configuration.)

- `CopyToPlugins` = `true`/`false`: HTTP Status DLLをBeat Saberのインストールにコピーするかどうかを指定します。`GameDirPath`に依存します。
(`CopyToPlugins` = `true`/`false`: Enable/disable copying of the HTTP Status DLLs to the Beat Saber installation. Depends on `GameDirPath`.)

また、[System.Data.SQLite.Core](https://system.data.sqlite.org/)ライブラリも使用しています。インストールはVisualStudioのNuGetパッケージマネージャからインストールして下さい。

---

Alternatively you can provide the game DLLs in the `libs/beatsaber` directory using the standard Beat Saber directory structure. For a full list see the [project file](BeatSaberHTTPStatus/BeatSaberHTTPStatusPlugin.csproj).

The following properties can be specified either in the `.csproj.user` file or through the command line (`/p:<name>=<value>`):

- `GameDirPath`: Path ending with a backslash pointing to the Beat Saber directory. Used to locate required game DLLs.

- `OutputZip` = `true`/`false`: Enable/disable generating the .zip file. Can be used to get a zip for the `Debug` configuration.

- `CopyToPlugins` = `true`/`false`: Enable/disable copying of HTTP Status DLLs to the Beat Saber installation. Depends on `GameDirPath`.

We also use the [System.Data.SQLite.Core](https://system.data.sqlite.org/) library. Installation is done through VisualStudio's NuGet package manager Please do.

## クレジット (Credits)

素晴らしいツールである[Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status)を製作したopl氏に感謝します。

sta氏製作の[websocket-sharp](https://github.com/sta/websocket-sharp)ライブラリを使用しています。

SQLite Development Team製作の[System.Data.SQLite.Core](https://system.data.sqlite.org/)ライブラリを使用しています。

---
Thanks to opl for creating the great tool, Beat Saber HTTP Status.

sta for the [websocket-sharp](https://github.com/sta/websocket-sharp) library.

SQLite Development Team's [System.Data.SQLite .Core](https://system.data.sqlite.org/) library.
