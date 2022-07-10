using System.Diagnostics;

namespace Utils {
    public static class StopwatchExtensions {
        public static double ToMetricTime(double elapsedTicks, TimeUnit unit) {
            long frequency = Stopwatch.Frequency;
            return unit switch {
                TimeUnit.Seconds => elapsedTicks / frequency,
                TimeUnit.Milliseconds => (elapsedTicks / frequency) * 1000,
                TimeUnit.Nanoseconds => (elapsedTicks / frequency) * 1000000000,
                _ => -1
            };
        }

        public enum TimeUnit {
            Seconds,
            Milliseconds,
            Nanoseconds
        }
    }
}