using System;
using System.Collections.Generic;
using Shared.FSM;

namespace Utils.Logger {
    public class GroupLoggingState : IState {
        public string StateName { get; }
        public List<Action> LogActions { get; set; } = new List<Action>();

        public void OnEnter() {
            
        }

        public void UpdateState(float deltaTime) {
            foreach (Action action in LogActions) {
                action?.Invoke();
            }
        }

        public void OnExit() {
            LogActions.Clear();
        }
    }
}