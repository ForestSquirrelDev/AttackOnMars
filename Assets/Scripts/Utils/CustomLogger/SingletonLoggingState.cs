using System;
using System.Collections.Generic;
using Shared.FSM;

namespace Utils.Logger {
    public class SingletonLoggingState : IState {
        public string StateName { get; }
        public Queue<Action> singletonReqs { get; set; } = new Queue<Action>();

        private Action currentLogAction;
        private int previousCount;

        public void OnEnter() {
            currentLogAction = singletonReqs.Dequeue();
            previousCount = singletonReqs.Count;
        }

        public void UpdateState(float deltaTime) {
            if (previousCount != singletonReqs.Count && singletonReqs.Count > 0) {
                currentLogAction = singletonReqs.Dequeue();
            }
            currentLogAction?.Invoke();
        }

        public void OnExit() {
            singletonReqs.Clear();
        }
    }
}