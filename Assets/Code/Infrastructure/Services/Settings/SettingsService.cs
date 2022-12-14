using Code.Configs;
using UnityEngine;

namespace Code.Infrastructure.Services.Settings
{
    public sealed class SettingsService :
        ISettingsService
    {
        private const string CONFIGS = "Configs/config main";

        private MainConfig _mainConfig;

        public SettingsService()
        {
            _mainConfig = Resources.Load<MainConfig>(CONFIGS);
        }

        public MainConfig MainConfig => _mainConfig;
    }
}