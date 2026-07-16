using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CareerQuest.Core
{
    public static class MyLogger
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            UnityEngine.Debug.Log($"{message} (at {filePath}:{line})");
        }

        [Conditional("UNITY_EDITOR")]
        public static void WarningLog(object message, [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            UnityEngine.Debug.LogWarning($"{message} (at {filePath}:{line})");
        }

        [Conditional("UNITY_EDITOR")]
        public static void ErrorLog(object message, [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            UnityEngine.Debug.LogError($"{message} (at {filePath}:{line})");
        }
    }
}