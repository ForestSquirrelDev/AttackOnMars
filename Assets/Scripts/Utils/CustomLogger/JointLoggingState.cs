using System;
using System.Collections.Generic;
using Shared.FSM;
using UnityEngine;
using static Utils.Logger.CustomLogger;

namespace Utils.Logger {
    public class JointLoggingState : IState {
        public string StateName { get; }
        public Queue<LogRequirement> Requirements { get; } = new Queue<LogRequirement>();
        public Dictionary<string, string> Logs { get; } = new Dictionary<string, string>();

        public void OnEnter() {
            
        }

        public void UpdateState(float deltaTime) {
            if (Requirements.Count > 0) {
                Requirements.Dequeue();
                foreach (var log in Logs) {
                    Debug.Log(log.Value);
                }
            }
        }

        public void OnExit() {
            Logs.Clear();
        }

        public void AddLog(string key, string log) {
            if (Logs.ContainsKey(key)) {
                Logs[key] = log;
            } else {
                Logs.Add(key, log);
            }
        }
    }
}