using HttpSiraStatus.Configuration;
using HttpSiraStatus.Installer;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
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
        public static string PluginVersion => s_metadata.HVersion.ToString();
        public static string GameVersion => Application.version;

        public string Name => Assembly.GetExecutingAssembly().GetName().Name;
        public static IPALogger Logger { get; private set; }
        private static PluginMetadata s_metadata;
        [Init]
        public void Init(IPALogger logger, Zenjector zenjector, PluginMetadata metadata, Config config)
        {
            Logger = logger;
            s_metadata = metadata;
            Logger.Debug("Logger Initialized.");
            zenjector.Install<HttpAppInstaller>(Location.App);
            zenjector.Install(Location.App, container =>
            {
                container.BindInterfacesAndSelfTo<PluginConfig>().FromInstance(config.Generated<PluginConfig>());
            });
            zenjector.Install<HttpMainInstaller>(Location.Menu);
            zenjector.Install<HttpPlayerInstaller>(Location.Player);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.Debug($"Game version : {GameVersion}");
        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }
        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}
