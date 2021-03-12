using HttpSiraStatus.Interfaces;
using System;
using System.Text;
using WebSocketSharp.Server;
using Zenject;

namespace HttpSiraStatus
{
    public class HTTPServer : IInitializable, IDisposable
    {
        private int ServerPort = 6557;

        private HttpServer server;
        [Inject]
        private IStatusManager statusManager;
        private bool disposedValue;

        public void OnHTTPGet(HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;

            if (req.RawUrl == "/status.json") {
                res.StatusCode = 200;
                res.ContentType = "application/json";
                res.ContentEncoding = Encoding.UTF8;

                var stringifiedStatus = Encoding.UTF8.GetBytes(statusManager.StatusJSON.ToString());

                res.ContentLength64 = stringifiedStatus.Length;
                res.Close(stringifiedStatus, false);

                return;
            }

            res.StatusCode = 404;
            res.Close();
        }
        public void Initialize()
        {
            server = new HttpServer(this.ServerPort);

            server.OnGet += (sender, e) =>
            {
                OnHTTPGet(e);
            };

            server.AddWebSocketService<StatusBroadcastBehavior>("/socket", initializer => initializer.SetStatusManager(this.statusManager));

            HttpSiraStatus.Plugin.Logger.Info("Starting HTTP server on port " + this.ServerPort);
            server.Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    HttpSiraStatus.Plugin.Logger.Info("Stopping HTTP server");
                    server.Stop();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~HTTPServer()
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
