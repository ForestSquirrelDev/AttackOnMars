using System;
using System.Threading.Tasks;
using Shared.FSM;
using UnityEngine;
using Random = System.Random;

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

        public static void Log(string log, LogOptions logOptions, string key = null) {
            switch (logOptions) {
                case LogOptions.Singleton:
                    singletonLogging.AddLogger(log);
                    singletonLogging.Requirements.Enqueue(new LogRequirement());
                    break;
                case LogOptions.Joint:
                    if (string.IsNullOrEmpty(key)) key = GetRandomString();
                    jointLogging.AddLog(key, log);
                    jointLogging.Requirements.Enqueue(new LogRequirement());
                    break;
            }
        }

        private static string GetRandomString() {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[8];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
        
        public struct LogRequirement {
            
        }
    }
    public enum LogOptions {
        Joint,
        Singleton
    }
}