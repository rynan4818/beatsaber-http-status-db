using BeatSaberHTTPStatus.Interfaces;
using BeatSaberHTTPStatus.Util;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Zenject;

namespace BeatSaberHTTPStatus
{
    public class StatusBroadcastBehavior : WebSocketBehavior
    {
		private IStatusManager statusManager;
        private bool disposedValue;

		public void SetStatusManager(IStatusManager statusManager)
        {
			this.statusManager = statusManager;
			this.statusManager.statusChange += OnStatusChange;
        }

		protected override void OnOpen()
		{
			base.OnOpen();
			JSONObject eventJSON = new JSONObject();

			eventJSON["event"] = "hello";
			eventJSON["time"] = new JSONNumber(Utility.GetCurrentTime());
			eventJSON["status"] = statusManager.statusJSON;

			SendAsync(eventJSON.ToString(), null);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			statusManager.statusChange -= OnStatusChange;
			base.OnClose(e);
		}

		public void OnStatusChange(IStatusManager statusManager, ChangedProperties changedProps, string cause)
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

			SendAsync(eventJSON.ToString(), null);
		}
    }
}
