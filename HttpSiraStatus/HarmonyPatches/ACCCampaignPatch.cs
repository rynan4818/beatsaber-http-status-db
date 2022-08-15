using HarmonyLib;
using HttpSiraStatus.Models;
using IPA.Loader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HttpSiraStatus.HarmonyPatches
{
    [HarmonyPatch]
    public class ACCCampaignPatch
    {
        private static readonly Type[] s_argumentTypes = new Type[] { typeof(ScoringElement) };

        /// <summary>
        /// パッチを当てるかどうか
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        [HarmonyPrepare]
        public static bool SetMultipliersPrefixPrepare(MethodBase original)
        {
            return SetMultipliersPrefixMethod(original) != null;
        }

        /// <summary>
        /// AccCampaignScoreSubmission.dllから対象のメソッド情報を取得します。
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        [HarmonyTargetMethod]
        public static MethodBase SetMultipliersPrefixMethod(MethodBase original)
        {
            if (original != null) {
                return original;
            }
            var scoreSaberInfo = PluginManager.GetPlugin("AccCampaignScoreSubmission");
            if (scoreSaberInfo == null) {
                Plugin.Logger.Info("AccCampaignScoreSubmission not loaded.");
                return null;
            }
            var accCampaignScoreSubmissionPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AccCampaignScoreSubmission.dll");
            Assembly accCampaignScoreSubmissionAssembly = null;
            try {
                accCampaignScoreSubmissionAssembly = Assembly.LoadFrom(accCampaignScoreSubmissionPath);
            }
            catch (FileNotFoundException) {
                Plugin.Logger.Info("AccCampaignScoreSubmission failed load");
                return null;
            }
            catch (Exception e) {
                Plugin.Logger.Error(e);
                return null;
            }
            var harmonyPatches = accCampaignScoreSubmissionAssembly.GetTypes().Select(x => new { type = x, patch = x.GetCustomAttribute(typeof(HarmonyPatch)) as HarmonyPatch }).Where(x => x.patch != null);
            foreach (var patchType in harmonyPatches) {
                if (patchType.patch.info.methodName != nameof(ScoringElement.SetMultipliers)) {
                    continue;
                }
                var methodInfos = patchType.type
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(x => x.Name == "Prefix" || x.GetCustomAttribute(typeof(HarmonyPrefix)) != null);

                foreach (var methodInfo in methodInfos) {
                    var arguments = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
                    if (arguments.SequenceEqual(s_argumentTypes)) {
                        Plugin.Logger.Info($"{patchType.type}");
                        Plugin.Logger.Info($"{methodInfo}");
                        return methodInfo;
                    }
                }
            }
            Plugin.Logger.Info("Not found target method.");
            return null;
        }

        /// <summary>
        /// AccCampaignScoreSubmissionでバグるらしい
        /// </summary>
        /// <param name="__0"><see cref="ScoringElement"/>__instance</param>
        /// <param name="__runOriginal"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        public static bool SetMultipliersPrefix(ScoringElement __0, ref bool __runOriginal)
        {
            __runOriginal = __0 != null && !(__0 is CustomBadCutScoringElement || __0 is CustomGoodCutScoringElement);
            return __runOriginal;
        }
    }
}
