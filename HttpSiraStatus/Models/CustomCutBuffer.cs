using Zenject;

namespace HttpSiraStatus.Models
{
    public class CustomCutBuffer : CutScoreBuffer
    {
        public NoteCutInfo NoteCutInfo { get; set; }
        public ICutScoreBufferDidFinishEvent CutBufferDidFinishEvent { get; set; }
        public void Initialize(NoteCutInfo noteCutInfo, int m, ICutScoreBufferDidFinishEvent e)
        {
            this._initialized = true;
            this._multiplier = m;
            this._saberSwingRatingCounter = noteCutInfo.swingRatingCounter;
            this._cutDistanceToCenter = noteCutInfo.cutDistanceToCenter;
            noteCutInfo.swingRatingCounter.RegisterDidChangeReceiver(this);
            noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(this);
            this.NoteCutInfo = noteCutInfo;
            this.RefreshScores();
            this.CutBufferDidFinishEvent = e;
            this.didFinishEvent.Add(this.CutBufferDidFinishEvent);
        }

        public void OnDespawned()
        {
            this.NoteCutInfo.swingRatingCounter.UnregisterDidFinishReceiver(this);
            this.NoteCutInfo.swingRatingCounter.UnregisterDidChangeReceiver(this);
            this.didFinishEvent.Remove(this.CutBufferDidFinishEvent);
        }

        public new class Pool : MemoryPool<NoteCutInfo, int, ICutScoreBufferDidFinishEvent, CustomCutBuffer>
        {
            protected override void Reinitialize(NoteCutInfo p1, int p2, ICutScoreBufferDidFinishEvent p3, CustomCutBuffer item) => item.Initialize(p1, p2, p3);
            protected override void OnDespawned(CustomCutBuffer item)
            {
                item.OnDespawned();
            }
            protected override void OnDestroyed(CustomCutBuffer item)
            {
                this.OnDespawned(item);
            }
        }
    }
}
