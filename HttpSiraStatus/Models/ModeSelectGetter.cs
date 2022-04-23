using System;
using Zenject;

namespace HttpSiraStatus.Models
{
    internal class ModeSelectGetter : IInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void Initialize()
        {
            this._mainMenuViewController.didFinishEvent += this.OnMainMenuViewController_didFinishEvent;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnMainMenuViewController_didFinishEvent(MainMenuViewController arg1, MainMenuViewController.MenuButton arg2)
        {
            switch (arg2) {
                case MainMenuViewController.MenuButton.SoloFreePlay:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.Party:
                    this._gameStatus.partyMode = true;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.BeatmapEditor:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.SoloCampaign:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.FloorAdjust:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.Quit:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.Multiplayer:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = true;
                    break;
                case MainMenuViewController.MenuButton.Options:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                case MainMenuViewController.MenuButton.HowToPlay:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
                default:
                    this._gameStatus.partyMode = false;
                    this._gameStatus.multiplayer = false;
                    break;
            }
            this._gameStatus.mode = $"{arg2}";
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private readonly GameStatus _gameStatus;
        private readonly MainMenuViewController _mainMenuViewController;
        private bool _disposedValue;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        public ModeSelectGetter(GameStatus gameStatus, MainMenuViewController mainMenuViewController)
        {
            this._gameStatus = gameStatus;
            this._mainMenuViewController = mainMenuViewController;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    this._mainMenuViewController.didFinishEvent -= this.OnMainMenuViewController_didFinishEvent;
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
        #endregion
    }
}
