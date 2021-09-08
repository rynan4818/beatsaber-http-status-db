using System.Collections.Generic;

namespace HttpSiraStatus.Models
{
    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/>用にハッシュ値を固定で返すラッパークラス
    /// </summary>
    public record NoteDataEntity
    {
        // C#の命名規則に違反するけど元々のNoteDataをラップするのでここは目をつむる
        public ColorType colorType { get; private set; }
        public NoteCutDirection cutDirection { get; private set; }
        public float timeToNextColorNote { get; private set; }
        public float timeToPrevColorNote { get; private set; }
        public NoteLineLayer noteLineLayer { get; private set; }
        public NoteLineLayer beforeJumpNoteLineLayer { get; private set; }
        public int flipLineIndex { get; private set; }
        public float flipYSide { get; private set; }
        public float duration { get; private set; }
        public bool skipBeforeCutScoring { get; private set; }
        public bool skipAfterCutScoring { get; private set; }
        public BeatmapObjectType beatmapObjectType { get; private set; }
        public float time { get; private set; }
        public int lineIndex { get; private set; }
        public NoteDataEntity(NoteData note, bool noArrow)
        {
            this.beatmapObjectType = note.beatmapObjectType;
            this.time = note.time;
            this.lineIndex = note.lineIndex;
            this.colorType = note.colorType;
            this.cutDirection = noArrow ? NoteCutDirection.Any : note.cutDirection;
            this.timeToNextColorNote = note.timeToNextColorNote;
            this.timeToPrevColorNote = note.timeToPrevColorNote;
            this.noteLineLayer = note.noteLineLayer;
            this.beforeJumpNoteLineLayer = note.beforeJumpNoteLineLayer;
            this.flipLineIndex = note.flipLineIndex;
            this.flipYSide = note.flipYSide;
            this.duration = note.duration;
            this.skipBeforeCutScoring = note.skipBeforeCutScoring;
            this.skipAfterCutScoring = note.skipAfterCutScoring;
        }
    }
}
