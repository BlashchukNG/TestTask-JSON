namespace Code.Infrastructure.States
{
    public interface IExitableState
    {
        void Exit();
    }

    public interface IState :
        IExitableState
    {
        void Enter();
    }

    public interface IPayloadedState<TPayload> :
        IExitableState
    {
        void Enter(TPayload payload);
    }
}