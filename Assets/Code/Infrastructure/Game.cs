using Code.Infrastructure.Services;
using Code.Infrastructure.States;
using Code.Logic;

namespace Code.Infrastructure
{
    public sealed class Game
    {
        public readonly GameStateMachine stateMachine;

        public Game(ICoroutineRunner coroutineRunner, LoadingCurtain loadingCurtain)
        {
            stateMachine = new GameStateMachine(new SceneLoader(coroutineRunner), loadingCurtain, ServiceLocator.Container, coroutineRunner);
        }
    }
}