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

        public new void Init(NoteCutInfo noteCutInfo, int m)
        {
            this._initialized = true;
            this._multiplier = multiplier;
            this._saberSwingRatingCounter = noteCutInfo.swingRatingCounter;
            this._cutDistanceToCenter = noteCutInfo.cutDistanceToCenter;
            noteCutInfo.swingRatingCounter.RegisterDidChangeReceiver(this);
            noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(this);
            this.NoteCutInfo = noteCutInfo;
            this.RefreshScores();
        }

        public new class Pool : MemoryPool<CustomCutBuffer>
        {

        }
    }
}
