using HttpSiraStatus.Models;
using Zenject;

namespace HttpSiraStatus.Installer
{
    public class HttpPlayerInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindMemoryPool<CustomGoodCutScoringElement, CustomGoodCutScoringElement.Pool>().WithInitialSize(16);
            this.Container.BindMemoryPool<NoteDataEntity, NoteDataEntity.Pool>().WithInitialSize(16);
            this.Container.BindInterfacesAndSelfTo<GamePlayDataManager>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
        }
    }
}
