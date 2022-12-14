using Code.Infrastructure.AssetManagement;
using UnityEngine;

namespace Code.Infrastructure.Factory
{
    public sealed class GameFactory :
        IGameFactory
    {
        private readonly IAssetProvider _assets;


        public GameFactory(IAssetProvider assets)
        {
            _assets = assets;
        }

        public GameObject CreateViewMain() => _assets.Instantiate(AssetsPath.VIEW_MAIN_PATH);
        public GameObject CreateViewUserData(Transform root) => _assets.Instantiate(AssetsPath.VIEW_USER_DATA_PATH, root);

        public void CleanUp()
        {
        }
    }
}