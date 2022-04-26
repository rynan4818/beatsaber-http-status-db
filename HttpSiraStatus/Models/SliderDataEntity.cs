using HttpSiraStatus.Interfaces;
using Zenject;

#pragma warning disable IDE1006 // 命名スタイル
namespace HttpSiraStatus.Models
{
    internal class SliderDataEntity : IBeatmapObjectEntity
    {
        // C#の命名規則に違反するけど元々のNoteDataをラップするのでここは目をつむる
        public ColorType colorType { get; private set; }
        public SliderData.Type sliderType { get; private set; }
        public bool hasHeadNote { get; private set; }
        public float headControlPointLengthMultiplier { get; private set; }
        public int headLineIndex { get; private set; }
        public NoteLineLayer headLineLayer { get; private set; }
        public NoteLineLayer headBeforeJumpLineLayer { get; private set; }
        public NoteCutDirection headCutDirection { get; private set; }
        public float headCutDirectionAngleOffset { get; private set; }
        public bool hasTailNote { get; private set; }
        public float tailTime { get; private set; }
        public int tailLineIndex { get; private set; }
        public float tailControlPointLengthMultiplier { get; private set; }
        public NoteLineLayer tailLineLayer { get; private set; }
        public NoteLineLayer tailBeforeJumpLineLayer { get; private set; }
        public NoteCutDirection tailCutDirection { get; private set; }
        public float tailCutDirectionAngleOffset { get; private set; }
        public SliderMidAnchorMode midAnchorMode { get; private set; }
        public int sliceCount { get; private set; }
        public float squishAmount { get; private set; }
        public float time { get; private set; }
        public int executionOrder { get; private set; }
        public int subtypeIdentifier { get; private set; }
        public BeatmapDataItem.BeatmapDataItemType type { get; private set; }
        public SliderDataEntity()
        {
        }
        public SliderDataEntity(SliderData note)
        {
            this.SetData(note);
        }

        public void SetData(SliderData note)
        {
            this.colorType = note.colorType;
            this.sliderType = note.sliderType;
            this.time = note.time;
            this.executionOrder = note.executionOrder;
            this.subtypeIdentifier = note.subtypeIdentifier;
            this.type = note.type;
            this.tailCutDirection = note.tailCutDirection;
            this.tailCutDirectionAngleOffset = note.tailCutDirectionAngleOffset;
            this.midAnchorMode = note.midAnchorMode;
            this.hasHeadNote = note.hasHeadNote;
            this.hasTailNote = note.hasTailNote;
            this.headCutDirectionAngleOffset = note.headCutDirectionAngleOffset;
            this.headLineIndex = note.headLineIndex;
            this.tailLineIndex = note.tailLineIndex;
            this.headLineLayer = note.headLineLayer;
            this.tailLineLayer = note.tailLineLayer;
            this.headBeforeJumpLineLayer = note.headBeforeJumpLineLayer;
            this.headCutDirection = note.headCutDirection;
            this.headControlPointLengthMultiplier = note.headControlPointLengthMultiplier;
            this.tailTime = note.tailTime;
            this.tailBeforeJumpLineLayer = note.tailBeforeJumpLineLayer;
            this.tailControlPointLengthMultiplier = note.tailControlPointLengthMultiplier;
            this.sliceCount = note.sliceCount;
            this.squishAmount = note.squishAmount;
        }
        public class Pool : MemoryPool<SliderData, SliderDataEntity>
        {
            protected override void Reinitialize(SliderData p1, SliderDataEntity item)
            {
                item.SetData(p1);
            }
        }
    }
}
