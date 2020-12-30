using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberHTTPStatus.Models
{
    public class NoteDataEntity
    {
        public int Index { get; private set; }
        public NoteData NoteData { get; private set; }

        public NoteDataEntity(int index, NoteData note)
        {
            this.Index = index;
            this.NoteData = note;
        }
    }
}
