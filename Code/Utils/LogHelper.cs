
namespace  IndustriesMeetsSunsetHarbor.Utils
{
    internal static class LogHelper
    {
        private const string Prefix = "IndustriesMeetsSunsetHarbor: ";

        public static void Information(string message, params object[] args)
        {
            var msg = Prefix + string.Format(message, args);
            UnityEngine.Debug.Log(msg);
        }

        public static void Information(bool shouldLog, string message, params object[] args)
        {
            if(shouldLog)
            {
                var msg = Prefix + string.Format(message, args);
                UnityEngine.Debug.Log(msg);
            }
        }

        public static void Warning(string message, params object[] args)
        {
            var msg = Prefix + string.Format(message, args);
            UnityEngine.Debug.LogWarning(msg);
        }

        public static void Warning(bool shouldLog, string message, params object[] args)
        {
            if(shouldLog)
            {
                var msg = Prefix + string.Format(message, args);
                UnityEngine.Debug.LogWarning(msg);
            }
        }

        public static void Error(string message, params object[] args)
        {
            var msg = Prefix + string.Format(message, args);
            UnityEngine.Debug.LogError(msg);
        }

        public static void Error(bool shouldLog, string message, params object[] args)
        {
            if(shouldLog)
            {
                var msg = Prefix + string.Format(message, args);
                UnityEngine.Debug.LogError(msg);
            }
        }
    }
}
