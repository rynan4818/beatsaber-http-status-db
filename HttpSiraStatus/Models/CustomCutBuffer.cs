using Zenject;

namespace HttpSiraStatus.Models
{
    public class CustomCutBuffer : CutScoreBuffer
    {
        public NoteCutInfo NoteCutInfo { get; set; }
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
            this.didFinishEvent.Add(e);
        }

        public new class Pool : MemoryPool<NoteCutInfo, int, ICutScoreBufferDidFinishEvent, CustomCutBuffer>
        {
            protected override void Reinitialize(NoteCutInfo p1, int p2, ICutScoreBufferDidFinishEvent p3, CustomCutBuffer item) => item.Initialize(p1, p2, p3);
        }
    }
}
