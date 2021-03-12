using System;
using System.Linq;
using UnityEngine;

namespace HttpSiraStatus.Util
{
    public static class Utility
    {
        public static long GetCurrentTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static bool NoteDataEquals(NoteData a, NoteData b)
        {
            if (a == null || b == null) {
                return false;
            }

            return a.time == b.time
                && a.lineIndex == b.lineIndex
                && a.noteLineLayer == b.noteLineLayer
                && a.colorType == b.colorType
                && a.cutDirection == b.cutDirection
                && a.duration == b.duration;
        }


        public static T FindFirstOrDefault<T>() where T : UnityEngine.Object
        {
            T obj = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            if (obj == null) {
                Plugin.Logger.Error("Couldn't find " + typeof(T).FullName);
                throw new InvalidOperationException("Couldn't find " + typeof(T).FullName);
            }
            return obj;
        }

        public static T FindFirstOrDefaultOptional<T>() where T : UnityEngine.Object
        {
            T obj = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            return obj;
        }

    }
}
