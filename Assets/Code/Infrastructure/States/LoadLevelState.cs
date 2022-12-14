using Code.Infrastructure.Datas;
using Code.Infrastructure.Factory;
using Code.Logic;
using Code.Views.ViewMain;
using UnityEngine;

namespace Code.Infrastructure.States
{
    public sealed class LoadLevelState :
        IPayloadedState<LoadLevelStateData>
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;

        private readonly IGameFactory _gameFactory;

        private LoadLevelStateData _data;

        public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, LoadingCurtain loadingCurtain, IGameFactory gameFactory)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _gameFactory = gameFactory;
        }

        public void Enter(LoadLevelStateData data)
        {
            _data = data;

            _loadingCurtain.Show();
            _gameFactory.CleanUp();

            Debug.Log(data.sceneName);
            _sceneLoader.Load(data.sceneName, onLoaded: OnLoaded);
        }

        public void Exit()
        {
            _loadingCurtain.Hide();
        }

        private void OnLoaded()
        {
            Debug.Log("Scene Loaded");

            InitGame();

            _gameStateMachine.Enter<GameLoopState>();
        }

        private void InitGame()
        {
            Debug.Log("Start Init");

            var main = _gameFactory.CreateViewMain().GetComponent<MainView>();
            main.Initialize(_data.userData);
        }
    }
}