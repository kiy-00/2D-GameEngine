using System;
using System.IO;
using System.Text;

namespace GameEngine.Common
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4,
        None = 5
    }

    /// <summary>
    /// 提供应用程序日志记录功能
    /// </summary>
    public static class Logger
    {
        #region 属性

        /// <summary>
        /// 获取或设置当前日志级别
        /// </summary>
        public static LogLevel Level { get; set; } = LogLevel.Info;

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public static string LogFilePath { get; private set; }

        /// <summary>
        /// 是否在控制台显示日志
        /// </summary>
        public static bool LogToConsole { get; set; } = true;

        /// <summary>
        /// 是否将日志写入文件
        /// </summary>
        public static bool LogToFile { get; set; } = false;

        #endregion

        #region 私有字段

        private static readonly object _lock = new object();
        private static bool _initialized = false;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        /// <param name="logFilePath">日志文件路径，如果为空则使用默认路径</param>
        public static void Initialize(string logFilePath = null)
        {
            if (_initialized)
                return;

            lock (_lock)
            {
                if (_initialized)
                    return;

                if (string.IsNullOrEmpty(logFilePath))
                {
                    // 使用默认路径
                    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string logDirectory = Path.Combine(appDirectory, "Logs");

                    if (!Directory.Exists(logDirectory))
                        Directory.CreateDirectory(logDirectory);

                    string fileName = $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                    LogFilePath = Path.Combine(logDirectory, fileName);
                }
                else
                {
                    LogFilePath = logFilePath;

                    // 确保目录存在
                    string directory = Path.GetDirectoryName(LogFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                _initialized = true;
            }
        }

        #endregion

        #region 日志方法

        /// <summary>
        /// 写入调试日志
        /// </summary>
        public static void Debug(string message) => Log(LogLevel.Debug, message);

        /// <summary>
        /// 写入信息日志
        /// </summary>
        public static void Info(string message) => Log(LogLevel.Info, message);

        /// <summary>
        /// 写入警告日志
        /// </summary>
        public static void Warning(string message) => Log(LogLevel.Warning, message);

        /// <summary>
        /// 写入错误日志
        /// </summary>
        public static void Error(string message) => Log(LogLevel.Error, message);

        /// <summary>
        /// 写入错误日志（包含异常信息）
        /// </summary>
        public static void Error(string message, Exception ex)
        {
            Log(LogLevel.Error, $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }

        /// <summary>
        /// 写入致命错误日志
        /// </summary>
        public static void Fatal(string message) => Log(LogLevel.Fatal, message);

        /// <summary>
        /// 写入致命错误日志（包含异常信息）
        /// </summary>
        public static void Fatal(string message, Exception ex)
        {
            Log(LogLevel.Fatal, $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        public static void Log(LogLevel level, string message)
        {
            if (level < Level)
                return;

            // 确保初始化
            if (!_initialized)
                Initialize();

            string formattedMessage = FormatMessage(level, message);

            // 写入控制台
            if (LogToConsole)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = GetConsoleColor(level);
                Console.WriteLine(formattedMessage);
                Console.ForegroundColor = originalColor;
            }

            // 写入文件
            if (LogToFile)
            {
                lock (_lock)
                {
                    try
                    {
                        File.AppendAllText(LogFilePath, formattedMessage + Environment.NewLine, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        // 如果写文件失败，尝试输出到控制台
                        if (LogToConsole)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed to write to log file: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取日志级别对应的控制台颜色
        /// </summary>
        private static ConsoleColor GetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                case LogLevel.Info:
                    return ConsoleColor.White;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Fatal:
                    return ConsoleColor.DarkRed;
                default:
                    return ConsoleColor.White;
            }
        }

        /// <summary>
        /// 格式化日志消息
        /// </summary>
        private static string FormatMessage(LogLevel level, string message)
        {
            string levelText = level.ToString().ToUpper();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"[{timestamp}] [{levelText}] {message}";
        }

        #endregion
    }
}