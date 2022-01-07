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
            e.JsonNode["time"].AsLong = Utility.GetCurrentTime();
            this.Send(e.JsonNode.ToString());
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Plugin.Logger.Debug("OnOpen call.");
            var eventJSON = new JSONObject();

            eventJSON["event"] = "hello";
            eventJSON["time"].AsLong = Utility.GetCurrentTime();
            eventJSON["status"] = this.statusManager.StatusJSON;
            eventJSON["other"] = this.statusManager.OtherJSON;

            this.SendAsync(eventJSON.ToString(), null);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Plugin.Logger.Debug("OnClose call.");
            this.statusManager.SendEvent -= this.OnSendEvent;
            base.OnClose(e);
        }
    }
}
