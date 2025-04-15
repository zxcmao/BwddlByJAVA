using System;
using System.IO;
using UnityEngine;


namespace GameClass
{
    /// <summary>
    /// 游戏日志系统自动写入游戏运行信息，记录异常和玩家行为
    /// </summary>
    public static class GameLogger
    {
        private static string logDirectory;
        private static string logFilePath;
        private static StreamWriter writer;

        // 限制单个日志文件最大大小（单位：字节）
        private const long MaxLogSize = 1 * 1024 * 1024; // 1MB

        static GameLogger()
        {
            InitLogger();
            Application.logMessageReceived += HandleUnityLog;
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        }

        private static void InitLogger()
        {
            logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(logDirectory);

            string logFileName = $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            logFilePath = Path.Combine(logDirectory, logFileName);
            writer = new StreamWriter(logFilePath, true);
            writer.AutoFlush = true;

            Log("[Logger] 日志系统初始化完成。");
            Log($"[Logger] 日志路径: {logFilePath}");
        }

        public static void Log(string message)
        {
            string timeStamped = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Debug.Log(timeStamped);
            writer.WriteLine(timeStamped);
        }

        public static void LogWarning(string message)
        {
            string timeStamped = $"[WARNING {DateTime.Now:HH:mm:ss}] {message}";
            Debug.LogWarning(timeStamped);
            writer.WriteLine(timeStamped);
        }

        public static void LogError(string message)
        {
            string timeStamped = $"[ERROR {DateTime.Now:HH:mm:ss}] {message}";
            Debug.LogError(timeStamped);
            writer.WriteLine(timeStamped);
        }

        public static void LogEvent(string category, string detail)
        {
            string log = $"[EVENT] [{category}] {detail}";
            Log(log);
        }

        private static void HandleUnityLog(string condition, string stackTrace, LogType type)
        {
            string logEntry = $"[{type}] {condition}";
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                logEntry += $"\n{stackTrace}";
            }
            writer.WriteLine(logEntry);

            CheckFileSizeLimit();
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError($"[CRASH] Unhandled Exception: {e.ExceptionObject}");
        }

        public static string GetLogFilePath() => logFilePath;

        public static void OpenLogDirectory()
        {
    #if UNITY_STANDALONE_WIN || UNITY_EDITOR
            System.Diagnostics.Process.Start("explorer.exe", logDirectory.Replace("/", "\\"));
    #elif UNITY_ANDROID
            Log("[Logger] 安卓设备上请使用文件管理器访问: " + logDirectory);
    #endif
        }

        private static void CheckFileSizeLimit()
        {
            try
            {
                FileInfo info = new FileInfo(logFilePath);
                if (info.Length > MaxLogSize)
                {
                    writer?.Close();
                    InitLogger(); // 创建新日志文件
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Logger] 检查日志大小失败: {e.Message}");
            }
        }

        public static void CloseLogger()
        {
            writer?.Flush();
            writer?.Close();
            writer = null;
        }
    }
}