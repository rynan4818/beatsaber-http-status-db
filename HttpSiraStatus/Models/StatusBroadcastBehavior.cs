using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace HttpSiraStatus
{
    public class StatusBroadcastBehavior : WebSocketBehavior
    {
        private IStatusManager statusManager;
        public void SetStatusManager(IStatusManager statusManager)
        {
            this.statusManager = statusManager;
            this.statusManager.SendEvent += this.OnSendEvent;
        }

        private void OnSendEvent(object sender, SendEventArgs e)
        {
            e.JsonNode["time"] = new JSONNumber(Utility.GetCurrentTime());
            this.Send(e.JsonNode.ToString());
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Plugin.Logger.Debug("OnOpen call.");
            var eventJSON = new JSONObject();

            eventJSON["event"] = "hello";
            eventJSON["time"] = new JSONNumber(Utility.GetCurrentTime());
            eventJSON["status"] = this.statusManager.StatusJSON;

            this.SendAsync(eventJSON.ToString(), null);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Plugin.Logger.Debug("OnClose call.");
            this.statusManager.SendEvent -= this.OnSendEvent;
            base.OnClose(e);
        }

        //public void OnStatusChange(object sender, StatusChangedEventArgs e)
        //{
        //          if (sender is IStatusManager statusManager) {
        //		var changedProps = e.ChangedProperties;

        //		JSONObject eventJSON = new JSONObject();
        //		eventJSON["event"] = e.Cause;
        //		eventJSON["time"] = new JSONNumber(Utility.GetCurrentTime());

        //		if (changedProps.game && changedProps.beatmap && changedProps.performance && changedProps.mod) {
        //			eventJSON["status"] = statusManager.statusJSON;
        //		}
        //		else {
        //			JSONObject status = new JSONObject();
        //			eventJSON["status"] = status;

        //			if (changedProps.game) status["game"] = statusManager.statusJSON["game"];
        //			if (changedProps.beatmap) status["beatmap"] = statusManager.statusJSON["beatmap"];
        //			if (changedProps.performance) status["performance"] = statusManager.statusJSON["performance"];
        //			if (changedProps.mod) {
        //				status["mod"] = statusManager.statusJSON["mod"];
        //				status["playerSettings"] = statusManager.statusJSON["playerSettings"];
        //			}
        //		}

        //		if (changedProps.noteCut) {
        //			eventJSON["noteCut"] = statusManager.noteCutJSON;
        //		}

        //		if (changedProps.beatmapEvent) {
        //			eventJSON["beatmapEvent"] = statusManager.beatmapEventJSON;
        //		}
        //		statusManager.JsonQueue.Enqueue(eventJSON);
        //		//SendAsync(eventJSON.ToString(), null);
        //	}
        //}
    }
}
