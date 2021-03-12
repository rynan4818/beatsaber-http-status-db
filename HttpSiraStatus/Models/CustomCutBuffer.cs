using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BeatSaberHTTPStatus.Models
{
    public class CustomCutBuffer : CutScoreBuffer
    {
        public NoteCutInfo NoteCutInfo { get; set; }
        public void Initialize(NoteCutInfo noteCutInfo, int m, ICutScoreBufferDidFinishEvent e)
        {
            this._initialized = true;
            this._multiplier = multiplier;
            this._saberSwingRatingCounter = noteCutInfo.swingRatingCounter;
            this._cutDistanceToCenter = noteCutInfo.cutDistanceToCenter;
            noteCutInfo.swingRatingCounter.RegisterDidChangeReceiver(this);
            noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(this);
            this.NoteCutInfo = noteCutInfo;
            this.RefreshScores();
            didFinishEvent.Add(e);
        }

        public new class Pool : MemoryPool<NoteCutInfo, int, ICutScoreBufferDidFinishEvent, CustomCutBuffer>
        {
            protected override void Reinitialize(NoteCutInfo p1, int p2, ICutScoreBufferDidFinishEvent p3, CustomCutBuffer item)
            {
                item.Initialize(p1, p2, p3);
            }
        }
    }
}
