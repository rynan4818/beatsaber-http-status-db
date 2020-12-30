using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberHTTPStatus.Models
{
    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/>用にハッシュ値を固定で返すラッパークラス
    /// </summary>
    public class NoteDataEntity
    {
        public NoteData NoteData { get; private set; }
        public bool NoArrow { get; private set; }

        public override bool Equals(object obj)
        {
            return this.Equals((NoteDataEntity)obj);
        }
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
            return (this.NoteData.time
                + this.NoteData.lineIndex
                + (int)this.NoteData.noteLineLayer
                + (int)this.NoteData.colorType
                + (this.NoArrow ? (int)NoteCutDirection.Any : (int)this.NoteData.cutDirection)
                + this.NoteData.duration).GetHashCode();
        }
        public NoteDataEntity(NoteData note, bool noArrow)
        {
            this.NoteData = note;
            this.NoArrow = noArrow;
        }
    }
}
