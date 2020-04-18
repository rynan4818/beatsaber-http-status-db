# Beat Saber HTTP Status +Database

このプラグインは、opl氏が製作の[Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status)の機能に、SQLite ver3 形式でのデータベース記録機能を追加したものです。
拙作のBeat Saber プレイ動画カットツール（[BS Movie Cut](https://github.com/rynan4818/bs-movie-cut)）で使用するために製作しました。
HTTP Statusの機能は丸々有してるため、置き換えて使用することを想定しています。

## インストール方法

1. [Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status)のインストール手順によって、HTTP Statusをインストールします。そして、HTTP Statusが正しく動作するか一度確認して下さい。

2. 次に[リリースページ](https://github.com/rynan4818/beatsaber-http-status-db/releases)から最新のリリースをダウンロードします。
（2020/4/18現在、BeatSaber 1.3.0～1.9.0 の各バージョンに対応したプラグインを用意しました、環境に合わせてダウンロード＆インストールして下さい。）

3. zipをBeat Saberフォルダに解凍します。
（SQLite.Interop.dll と System.Data.SQLite.dll を Beat Saber\Beat Saber_Data\Managed フォルダにコピーし、Beat Saber\Plugins フォルダの BeatSaberHTTPStatus.dll を、本リリースのファイルと差し替えて下さい）

4. 一度Beat Saberを起動すると、UserData フォルダに下記内容の `movie_cut_record.json` ファイルが作成されます。
必要に応じて設定値を変更して下さい。[BS Movie Cut](https://github.com/rynan4818/bs-movie-cut)を使用すれば、GUI画面で設定可能です。
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
### movie_cut_record.json ファイルの各項目の説明
- dbfile ・・・ 記録するデータベースファイルの保存場所を変更する場合に指定します。nullの場合はUserDataフォルダにbeatsaber.dbのファイル名で記録します。変更する場合は、データベースファイルをフルパスで記載して下さい。パスの\マークは\\\\と記載して下さい。

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

以下は[Beat Saber Overlay no score](https://github.com/rynan4818/beat-saber-overlay-noscore)（Beat Saber Overlayのスコア表示を消したVer）を使用し、記録するデータベースファイルの保存先を変更した設定例です。

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

## 開発者向け

### HTTP Statusの使用

HTTP Statusのプロコトルのドキュメントは[protocol.md](protocol.md)にあります。

オリジナルの HTTP Statusから変更はありませんが、2020/04/12現在(HTTP Status v1.10.0) Note cut objectのfinalScoreには、カット後30点が含まれないため、本ツールでは含まれるように修正してあります。

### データベースの使用

データベースファイルは `MovieCutRecord` テーブルにHTTP Statusの `Status object` の内容が記録され、`NoteScore` テーブルに `Note cut object` の内容が記録されます。各カラム名はHTTP Statusプロコトルの各項目名と基本的に一致しています。但し、一部項目名の衝突があるため名称を変更していますが、見れば判るカラム名にしてあります。

### 本ツールへの貢献

初心者向けのビルド方法の[ガイド](https://drive.google.com/open?id=1g2XpANHnplUtqvxoTm_-htHh2joN-qFQDTtr0Aez2Y4)を作成しました。

このプロジェクトは、`websocket-sharp` ライブラリをGitのサブモジュールとして使用します。
`websocket-sharp` ライブラリも含めてダウンロードするには、`git submodule update --init --recursive` を使用するか、`--recursive`　フラグを付けてリポジトリをクローンして下さい。

このプロジェクトをビルドするには、 `BeatSaberHTTPStatus/BeatSaberHTTPStatusPlugin.csproj.user` ファイルを作成し、Beat Saberがインストールされた場所を指定する必要があります。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- Change this path if necessary. Make sure it ends with a backslash. -->
    <GameDirPath>C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\</GameDirPath>
  </PropertyGroup>
</Project>
```

または、libs\beatsaberフォルダにBeatSaberのインストールフォルダ構造を作成して、使用するDLLファイルを配置することもできます。その際に必要なDLLファイルは[プロジェクトファイル](BeatSaberHTTPStatus/BeatSaberHTTPStatusPlugin.csproj)を参照して下さい。

また、[System.Data.SQLite.Core](https://system.data.sqlite.org/)ライブラリも使用しています。インストールはVisualStudioのNuGetパッケージマネージャからインストールして下さい。

pull request をする前に、[投稿ガイド](CONTRIBUTING.md)を読んで下さい。

## クレジット

素晴らしいツールである[Beat Saber HTTP Status](https://github.com/opl-/beatsaber-http-status)を製作したopl氏に感謝します。

sta氏製作の[websocket-sharp](https://github.com/sta/websocket-sharp)ライブラリを使用しています。

SQLite Development Team製作の[System.Data.SQLite.Core](https://system.data.sqlite.org/)ライブラリを使用しています。
