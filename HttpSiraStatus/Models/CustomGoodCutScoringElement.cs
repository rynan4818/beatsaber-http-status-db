using UnityEngine;

namespace HttpSiraStatus.Models
{
    public class CustomGoodCutScoringElement : GoodCutScoringElement, ICutScoreBufferDidFinishReceiver
    {
        public NoteDataEntity NoteDataEntity { get; } = new NoteDataEntity();
        public Transform NoteTransform { get; private set; }
        public void Init(in NoteCutInfo noteCutInfo, NoteController controller, bool noArrow)
        {
            this.NoteDataEntity.SetData(controller.noteData, noArrow);
            this.NoteTransform = controller.noteTransform;
            base.Init(noteCutInfo);
        }
        public new class Pool : ScoringElement.Pool<CustomGoodCutScoringElement>
        {
        }
    }
}
