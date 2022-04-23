using HttpSiraStatus.Models;

namespace HttpSiraStatus.Installer
{
    internal class HttpMainInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<ModeSelectGetter>().AsCached().NonLazy();
        }
    }
}
