using HttpSiraStatus.Models;
using SiraUtil;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using UnityEngine;

namespace HttpSiraStatus.Installer
{
    public class HttpPlayerInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindMemoryPool<CustomCutBuffer, CustomCutBuffer.Pool>().WithInitialSize(90);
            this.Container.BindMemoryPool<NoteDataEntity, NoteDataEntity.Pool>().WithInitialSize(16);
            this.Container.BindInterfacesAndSelfTo<GamePlayDataManager>().FromNewComponentOn(new GameObject(nameof(GamePlayDataManager))).AsCached();
        }
    }
}
