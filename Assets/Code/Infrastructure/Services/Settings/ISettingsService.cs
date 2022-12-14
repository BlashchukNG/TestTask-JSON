using Code.Configs;

namespace Code.Infrastructure.Services.Settings
{
    public interface ISettingsService :
        IService
    {
        MainConfig MainConfig { get; }
    }
}