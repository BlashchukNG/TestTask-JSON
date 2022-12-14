using Code.Infrastructure.States;
using Code.Logic;
using UnityEngine;

namespace Code.Infrastructure
{
    public sealed class GameBootstrapper :
        MonoBehaviour,
        ICoroutineRunner
    {
        [SerializeField] private LoadingCurtain _curtain;

        private Game _game;

        private void Awake()
        {
            _game = new Game(coroutineRunner: this, Instantiate(_curtain));
            _game.stateMachine.Enter<BootstrapState>();

            DontDestroyOnLoad(this);
        }

        public Coroutine StartCoroutine(Coroutine coroutine) => StartCoroutine(coroutine);
        public void StopAllCoroutine() => StopAllCoroutines();
    }
}