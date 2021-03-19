using System.Collections.Generic;

namespace HttpSiraStatus.Models
{
    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/>用にハッシュ値を固定で返すラッパークラス
    /// </summary>
    public class NoteDataEntity
    {
        public NoteData NoteData { get; private set; }
        public bool NoArrow { get; private set; }

        public override bool Equals(object obj) => this.Equals((NoteDataEntity)obj);
        private bool Equals(NoteDataEntity entity)
        {
            if (entity == null) {
                return false;
            }

            return this.NoteData.time == entity.NoteData.time
                && this.NoteData.lineIndex == entity.NoteData.lineIndex
                && this.NoteData.noteLineLayer == entity.NoteData.noteLineLayer
                && this.NoteData.colorType == entity.NoteData.colorType
                && (this.NoteData.cutDirection == entity.NoteData.cutDirection || this.NoArrow)
                && this.NoteData.duration == entity.NoteData.duration;
        }
        public override int GetHashCode()
        {
            var enumTmp = ((int)this.NoteData.noteLineLayer
                | ((int)this.NoteData.colorType + 1) << 2
                | (this.NoArrow ? (int)NoteCutDirection.Any : (int)this.NoteData.cutDirection) << 4);
            return (this.NoteData.time
                + this.NoteData.lineIndex
                + enumTmp
                + this.NoteData.duration).GetHashCode();
        }
        public NoteDataEntity(NoteData note, bool noArrow)
        {
            this.NoteData = note;
            this.NoArrow = noArrow;
        }
    }
}
