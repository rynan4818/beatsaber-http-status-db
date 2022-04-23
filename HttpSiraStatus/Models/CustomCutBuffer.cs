using System;
using Zenject;

namespace HttpSiraStatus.Models
{
    public class CustomCutBuffer : CutScoreBuffer
    {
        public NoteCutInfo NoteCutInfo { get; private set; }
        public ICutScoreBufferDidFinishReceiver FinishEvent { get; private set; }
        public NoteController NoteController { get; private set; }

        public void Init(in NoteCutInfo noteCutInfo, int multiplier, NoteController controller, ICutScoreBufferDidFinishReceiver e)
        {
            this.FinishEvent = e;
            this.NoteController = controller;
            this._didFinishEvent?.Add(this.FinishEvent);
            this.NoteCutInfo = noteCutInfo;
            base.Init(noteCutInfo);
        }

        public void Reflesh()
        {
            this._didFinishEvent?.Remove(this.FinishEvent);
            this.NoteController = null;
            this.FinishEvent = null;
        }
        public class Pool : MemoryPool<NoteCutInfo, int, NoteController, ICutScoreBufferDidFinishReceiver, CustomCutBuffer>, IDisposable
        {
            // GCに勝手に回収されない用
            private readonly LazyCopyHashSet<CustomCutBuffer> _activeItems = new LazyCopyHashSet<CustomCutBuffer>(256);
            private bool _disposeValue = false;
            protected override void Reinitialize(NoteCutInfo p1, int p2, NoteController p3, ICutScoreBufferDidFinishReceiver p4, CustomCutBuffer item)
            {
                item.Init(p1, p2, p3, p4);
            }

            protected override void OnSpawned(CustomCutBuffer item)
            {
                this._activeItems.Add(item);
            }
            protected override void OnDespawned(CustomCutBuffer item)
            {
                item.Reflesh();
                this._activeItems.Remove(item);
            }
            protected override void OnDestroyed(CustomCutBuffer item)
            {
                item.Reflesh();
                this._activeItems.Remove(item);
            }

            protected virtual void Dispose(bool disposing)
            {
                try {
                    Plugin.Logger.Debug("Dispose()");
                    if (!this._disposeValue) {
                        foreach (var buff in this._activeItems.items) {
                            buff.Reflesh();
                        }
                        this._activeItems.items.Clear();
                        this._disposeValue = true;
                    }
                }
                catch (Exception e) {
                    Plugin.Logger.Error(e);
                }
            }

            ~Pool()
            {
                this.Dispose(false);
            }

            public new void Dispose()
            {
                this.Dispose(true);
                base.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
