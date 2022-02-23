namespace Shared.FSM {
    public interface IState {
        public string StateName { get; }
        public void OnEnter();
        public void UpdateState(float deltaTime);
        public void OnExit();
    }
}