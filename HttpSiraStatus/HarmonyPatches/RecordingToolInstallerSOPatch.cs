using HarmonyLib;
using Zenject;

namespace HttpSiraStatus.HarmonyPatches
{
    [HarmonyPatch(typeof(RecordingToolInstallerSO), nameof(RecordingToolInstallerSO.InstallDependencies))]
    internal class RecordingToolInstallerSOPatch
    {
        [HarmonyPrefix]
        public static void InstallDependenciesPrefix(RecordingToolInstallerSO __instance, DiContainer container, ProgramArguments programArguments, BeatmapCharacteristicCollection beatmapCharacteristicCollection, ref bool __runOriginal)
        {
            if (__runOriginal) {
                var recordingToolManager = new RecordingToolManager(programArguments, __instance._recordingToolResourceContainer, beatmapCharacteristicCollection, container);
                container.QueueForInject(recordingToolManager);
                _ = container.BindInterfacesAndSelfTo<RecordingToolManager>().FromInstance(recordingToolManager).AsSingle();
                _ = container.Bind<IBeatSaberLogger>().WithId("RecordingTool").FromInstance(recordingToolManager.logger).AsSingle();
                _ = container.Bind<IPosesSerializer>().FromInstance(recordingToolManager.posesSerializer).AsSingle();
                __runOriginal = false;
            }
        }
    }
}
