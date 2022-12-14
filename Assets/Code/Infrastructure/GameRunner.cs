using UnityEngine;

namespace Code.Infrastructure
{
    public sealed class GameRunner :
        MonoBehaviour
    {
        [SerializeField] private GameBootstrapper _bootstrapper;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            
            var bootstrapper = FindObjectOfType<GameBootstrapper>();
            if (bootstrapper == null) Instantiate(_bootstrapper);
        }
    }
}