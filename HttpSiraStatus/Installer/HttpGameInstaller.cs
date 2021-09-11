using HttpSiraStatus.Models;

namespace HttpSiraStatus.Installer
{
    public class HttpGameInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<GamePlayDataManager>().AsCached();
            this.Container.BindMemoryPool<CustomCutBuffer, CustomCutBuffer.Pool>().WithInitialSize(90);
            this.Container.BindMemoryPool<NoteDataEntity, NoteDataEntity.Pool>().WithInitialSize(16);
        }
    }
}
