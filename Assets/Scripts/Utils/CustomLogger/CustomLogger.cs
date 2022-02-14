using System;
using System.Threading.Tasks;
using Shared.FSM;
using UnityEngine;

namespace Utils.Logger {
    public static class CustomLogger {
        private static StateController stateController = new StateController();
        private static JointLoggingState jointLogging;
        private static SingletonLoggingState singletonLogging;

        private static bool SingletonLoggingRequired => !string.IsNullOrEmpty(singletonLogging?.log);
        private static bool JointLoggingRequired => string.IsNullOrEmpty(singletonLogging?.log);

        static CustomLogger() {
            jointLogging = new JointLoggingState();
            singletonLogging = new SingletonLoggingState();
            stateController.AddHighPriorityTransition(singletonLogging, () => SingletonLoggingRequired);
            stateController.AddTransition(singletonLogging, jointLogging, () => JointLoggingRequired);
            stateController.SetState(jointLogging);
            _ = UpdateLoop();
        }

        private static async Task UpdateLoop() {
            if (!Application.isEditor) return;
            while (Application.isPlaying) {
                stateController.Update(0);
                await Task.Delay(1);
            }
        }

        public static void Log(string key,string log, Options options) {
            switch (options) {
                case Options.Singleton:
                    singletonLogging.requirements.Enqueue(new LogRequirement());
                    singletonLogging.AddLogger(log);
                    break;
                case Options.Joint:
                    jointLogging.AddLog(key, log);
                    jointLogging.Requirements.Enqueue(new LogRequirement());
                    break;
            }
        }

        public enum Options {
            Joint,
            Singleton
        }

        public struct LogRequirement {
            
        }
    }
}