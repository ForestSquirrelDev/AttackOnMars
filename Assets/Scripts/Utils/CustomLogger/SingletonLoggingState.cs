using System;
using System.Collections.Generic;
using Shared.FSM;
using UnityEngine;
using static Utils.Logger.CustomLogger;

namespace Utils.Logger {
    public class SingletonLoggingState : IState {
        public string StateName { get; }
        public Queue<LogRequirement> Requirements { get; } = new Queue<LogRequirement>();
        public string log { get; private set; }

        private Action currentLogAction;
        private int previousCount;

        public void OnEnter() {
            
        }

        public void UpdateState(float deltaTime) {
            if (Requirements.Count > 0) {
                Requirements.Dequeue();
                Debug.Log(log);
            }
        }

        public void OnExit() {
            log = "";
            Requirements.Clear();
        }

        public void AddLogger(string log) {
            this.log = log;
        }
    }
}