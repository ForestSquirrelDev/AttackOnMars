using System;
using System.Threading.Tasks;
using Shared.FSM;
using UnityEngine;

namespace Utils.Logger {
    public static class CustomLogger {
        private static StateController stateController = new StateController();
        private static JointLoggingState jointLogging;
        private static GroupLoggingState groupLogging;
        private static SingletonLoggingState singletonLogging;

        private static bool SingletonLoggingRequired => singletonLogging?.singletonReqs.Count > 0;
        private static bool GroupLoggingRequired => groupLogging?.LogActions.Count > 0;
        private static bool JointLoggingRequired => groupLogging?.LogActions.Count == 0 && singletonLogging?.singletonReqs.Count == 0;

        static CustomLogger() {
            jointLogging = new JointLoggingState();
            groupLogging = new GroupLoggingState();
            singletonLogging = new SingletonLoggingState();
            stateController.AddHighPriorityTransition(singletonLogging, () => SingletonLoggingRequired);
            stateController.AddTransition(jointLogging, groupLogging, () => GroupLoggingRequired);
            stateController.AddTransition(groupLogging, jointLogging, () => JointLoggingRequired);
            stateController.AddTransition(singletonLogging, jointLogging, () => JointLoggingRequired);
            stateController.AddTransition(singletonLogging, groupLogging, () => GroupLoggingRequired);
            stateController.SetState(jointLogging);
            _ = UpdateLoop();
        }

        private static async Task UpdateLoop() {
            if (!Application.isEditor) return;
            while (Application.isPlaying) {
                stateController.Update(Time.fixedDeltaTime);
                Debug.Log($"Joint loggers count: {jointLogging.LogActions.Count}. Singleton loggers count: {singletonLogging.singletonReqs.Count}");
                await Task.Delay(20);
            }
        }

        public static void AddSingletonLogger(Action logAction) {
            if (singletonLogging.singletonReqs.Contains(logAction)) return;
            singletonLogging.singletonReqs.Enqueue(logAction);
        }

        public static void AddGroupLogger(Action logAction) {
            if (groupLogging.LogActions.Contains(logAction)) return;
            groupLogging.LogActions.Add(logAction);
        }

        public static void AddJointLogger(Action logAction) {
            if (groupLogging.LogActions.Contains(logAction)) return;
            jointLogging.LogActions.Add(logAction);
        }

        private static void Log(string s) {
            
        }
    }
}