using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Factory;
using Code.Infrastructure.Services;
using Code.Infrastructure.Services.JsonLoad;
using Code.Infrastructure.Services.Settings;

namespace Code.Infrastructure.States
{
    public sealed class BootstrapState :
        IState
    {
        private const string Initial = "Initial";

        private readonly GameStateMachine _gameStateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly ServiceLocator _services;
        private readonly ICoroutineRunner _coroutineRunner;

        public BootstrapState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, ServiceLocator services, ICoroutineRunner coroutineRunner)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _services = services;
            _coroutineRunner = coroutineRunner;

            RegisterServices();
        }

        public void Enter()
        {
            _sceneLoader.Load(Initial, onLoaded: EnterLoadScene);
        }

        public void Exit()
        {
        }

        private void EnterLoadScene()
        {
            _gameStateMachine.Enter<LoadJsonState>();
        }

        private void RegisterServices()
        {
            _services.RegisterSingle<ISettingsService>(new SettingsService());
            _services.RegisterSingle<IAssetProvider>(new AssetProvider());
            _services.RegisterSingle<IJsonLoadService>(new JsonLoadService(_coroutineRunner));
            _services.RegisterSingle<IGameFactory>(new GameFactory(_services.Single<IAssetProvider>()));
        }
    }
}