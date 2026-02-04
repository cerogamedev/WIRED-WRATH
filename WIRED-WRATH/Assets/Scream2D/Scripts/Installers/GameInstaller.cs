using UnityEngine;
using Zenject;
using Scream2D.Systems;

namespace Scream2D.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Debug.Log("Scream 2D: Global Systems Initialized.");
            
            Container.Bind<ScreamMeter>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LevelManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<Controllers.PlayerController>().FromComponentInHierarchy().AsSingle();
        }
    }
}
