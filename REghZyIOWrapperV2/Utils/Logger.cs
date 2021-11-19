using System;

namespace REghZyIOWrapperV2.Utils {
    public static class Logger {
        private static Action<string> logCallback;
        private static bool canLog;

        public static Action<string> Callback {
            get => Logger.logCallback;
            set {
                if (value == null) {
                    throw new NullReferenceException("Log callback cannot be null");
                }

                Logger.logCallback = value;
            }
        }

        public static bool CanLog {
            get => Logger.canLog;
            set => Logger.canLog = value;
        }

        static Logger() {
            Callback = Console.WriteLine;
            canLog = true;
        }

        public static void LogRaw(string line) {
            if (canLog) {
                logCallback(line);
            }
        }

        public static void LogNamed(string name, string line) {
            LogRaw($"{DateTime.Now.ToString("T")} [{name}] {line}");
        }

        public static void LogDated(string line) {
            LogRaw($"{DateTime.Now.ToString("T")} [General] {line}");
        }

        public static void LogNameDated(string name, string line) {
            LogRaw($"{DateTime.Now.ToString("T")} [{name}] {line}");
        }
    }
}
