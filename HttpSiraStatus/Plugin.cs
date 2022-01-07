using HarmonyLib;
using HttpSiraStatus.Installer;
using IPA;
using SiraUtil.Zenject;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace HttpSiraStatus
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        /// <summary>
        /// Populated by MSBuild
        /// </summary>
        public static string PluginVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string GameVersion => Application.version;

        public string Name => Assembly.GetExecutingAssembly().GetName().Name;
        public static IPALogger Logger { get; private set; }

        public const string HARMONY_ID = "HttpSiraStatus.com.github.denpadokei";
        private Harmony _harmony;
        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            Logger = logger;
            Logger.Debug("Logger Initialized.");
            zenjector.Install<HttpPlayerInstaller>(Location.Player);
            zenjector.Install<HttpAppInstaller>(Location.App);
            this._harmony = new Harmony(HARMONY_ID);
        }

        [OnStart]
        public void OnApplicationStart() => Logger.Debug($"Game version : {GameVersion}");

        [OnExit]
        public void OnApplicationQuit()
        {

        }
        [OnEnable]
        public void OnEnable()
        {
            this._harmony?.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            this._harmony?.UnpatchSelf();
        }
    }
}
