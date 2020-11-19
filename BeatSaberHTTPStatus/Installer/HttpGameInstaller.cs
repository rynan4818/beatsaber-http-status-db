using BeatSaberHTTPStatus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberHTTPStatus.Installer
{
    public class HttpGameInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<GamePlayDataManager>().AsTransient();
        }
    }
}
