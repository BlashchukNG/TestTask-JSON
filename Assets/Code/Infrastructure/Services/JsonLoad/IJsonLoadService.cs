using System;
using Code.Infrastructure.Datas;

namespace Code.Infrastructure.Services.JsonLoad
{
    public interface IJsonLoadService :
        IService
    {
        void Load(string url, Action<UserData[]> success, Action failed);
    }
}