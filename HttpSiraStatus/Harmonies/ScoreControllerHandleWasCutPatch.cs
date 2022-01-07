using HarmonyLib;
using System;
using UnityEngine.SceneManagement;

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

#if DEBUG
    [HarmonyPatch(typeof(DisableGCWhileEnabled), nameof(DisableGCWhileEnabled.OnEnable))]
    public class DisableGCWhileEnabledPatch
    {
        public static void Prefix()
        {
            Plugin.Logger.Debug($"OnEnable():{SceneManager.GetActiveScene().name}");
        }
    }

    [HarmonyPatch(typeof(DisableGCWhileEnabled), nameof(DisableGCWhileEnabled.OnDisable))]
    public class EnableGCWhileDisabledPatch
    {
        public static void Prefix()
        {
            Plugin.Logger.Debug($"OnDisable():{SceneManager.GetActiveScene().name}");
        }
    }
#endif



    public delegate void HandleNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo);
}
