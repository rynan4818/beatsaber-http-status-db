using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberHTTPStatus.Interfaces;
using BeatSaberHTTPStatus.Util;
using SimpleJSON;
using Zenject;

namespace BeatSaberHTTPStatus {
	public class HTTPServer : IHTTPServer, IInitializable, IDisposable
	{
		private int ServerPort = 6557;
		[Inject]
		private IStatusManager statusManager;
		private UdpClient udpClient;
		private Thread _workerThread;
        private bool disposedValue;
		public void Initialize()
		{
			udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1/socket"), this.ServerPort));
			this._workerThread = new Thread(new ThreadStart(() => this.Receive()));
			statusManager.statusChange += this.OnStatusChange;
			BeatSaberHTTPStatus.Plugin.log.Info("Starting HTTP server on port " + ServerPort);
			this._workerThread.Start();
		}

		void Receive()
        {
            while (true) {
                try {
					IPEndPoint endp = null;
					var req = this.udpClient.Receive(ref endp);
					if (Regex.IsMatch(endp.Address.ToString(), "/status\\.json$")) {
						var stringifiedStatus = Encoding.UTF8.GetBytes(statusManager.statusJSON.ToString());
						udpClient.SendAsync(stringifiedStatus, stringifiedStatus.Length, endp);
						udpClient.Close();
						continue;
					}
					udpClient.Close();
				}
                catch (Exception e) {
					Plugin.log.Error(e);
                }
			}
        }

		protected void OnOpen()
		{
			JSONObject eventJSON = new JSONObject();

			eventJSON["event"] = "hello";
			eventJSON["time"] = new JSONNumber(Utility.GetCurrentTime());
			eventJSON["status"] = statusManager.statusJSON;
			var buff = System.Text.Encoding.UTF8.GetBytes(eventJSON.ToString());
			udpClient.Send(buff, buff.Length);
		}

		public void OnStatusChange(StatusManager statusManager, ChangedProperties changedProps, string cause)
		{
			JSONObject eventJSON = new JSONObject();
			eventJSON["event"] = cause;
			eventJSON["time"] = new JSONNumber(Utility.GetCurrentTime());

			if (changedProps.game && changedProps.beatmap && changedProps.performance && changedProps.mod) {
				eventJSON["status"] = statusManager.statusJSON;
			}
			else {
				JSONObject status = new JSONObject();
				eventJSON["status"] = status;

				if (changedProps.game) status["game"] = statusManager.statusJSON["game"];
				if (changedProps.beatmap) status["beatmap"] = statusManager.statusJSON["beatmap"];
				if (changedProps.performance) status["performance"] = statusManager.statusJSON["performance"];
				if (changedProps.mod) {
					status["mod"] = statusManager.statusJSON["mod"];
					status["playerSettings"] = statusManager.statusJSON["playerSettings"];
				}
			}

			if (changedProps.noteCut) {
				eventJSON["noteCut"] = statusManager.noteCutJSON;
			}

			if (changedProps.beatmapEvent) {
				eventJSON["beatmapEvent"] = statusManager.beatmapEventJSON;
			}
			var buff = System.Text.Encoding.UTF8.GetBytes(eventJSON.ToString());
			_ = udpClient.SendAsync(buff, buff.Length);
		}

		protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
					// TODO: マネージド状態を破棄します (マネージド オブジェクト)
					BeatSaberHTTPStatus.Plugin.log.Info("Stopping HTTP server");
					statusManager.statusChange -= this.OnStatusChange;
					this._workerThread.Abort();
					this.udpClient.Dispose();
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
