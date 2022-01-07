using HarmonyLib;
using System;

namespace HttpSiraStatus.Harmonies
{
    [HarmonyPatch(typeof(ScoreController), nameof(ScoreController.HandleNoteWasCut), new Type[] { typeof(NoteController), typeof(NoteCutInfo) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
    public class ScoreControllerHandleWasCutPatch
    {
        public static void Postfix(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            NoteWasCut?.Invoke(noteController, in noteCutInfo);
        }

        public static event HandleNoteWasCut NoteWasCut;
    }
    public delegate void HandleNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo);
}
