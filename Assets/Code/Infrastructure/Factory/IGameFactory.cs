using Code.Infrastructure.Services;
using UnityEngine;

namespace Code.Infrastructure.Factory
{
    public interface IGameFactory :
        IService
    {
        GameObject CreateViewMain();
        GameObject CreateViewUserData(Transform root);
        void CleanUp();
    }
}