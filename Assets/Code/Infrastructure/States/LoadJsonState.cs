using Code.Infrastructure.Datas;
using Code.Infrastructure.Services.JsonLoad;
using Code.Infrastructure.Services.Settings;
using UnityEngine;

namespace Code.Infrastructure.States
{
    public sealed class LoadJsonState :
        IState
    {
        private const string SCENE_NAME = "Initial";

        private readonly GameStateMachine _gameStateMachine;
        private readonly ISettingsService _settingsService;
        private readonly IJsonLoadService _jsonLoadService;

        public LoadJsonState(GameStateMachine gameStateMachine, ISettingsService settingsService, IJsonLoadService jsonLoadService)
        {
            _gameStateMachine = gameStateMachine;
            _settingsService = settingsService;
            _jsonLoadService = jsonLoadService;
        }

        public void Exit()
        {
        }

        public void Enter()
        {
            _jsonLoadService.Load(_settingsService.MainConfig.url, LoadLevel, JsonLoadFailed);
        }

        private void LoadLevel(UserData[] data)
        {
            Debug.Log("JSON Loaded");
            
            _gameStateMachine.Enter<LoadLevelState, LoadLevelStateData>(new LoadLevelStateData(SCENE_NAME, data));
        }

        private void JsonLoadFailed()
        {
        }
    }
}