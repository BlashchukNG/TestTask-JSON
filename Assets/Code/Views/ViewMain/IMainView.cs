using Code.Infrastructure.Datas;
using Code.Views.ViewUserData;
using UnityEngine;

namespace Code.Views.ViewMain
{
    public interface IMainView
    {
        Transform RootUserData { get; }
        void Initialize(UserData[] data);
    }
}