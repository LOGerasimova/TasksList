using System.Runtime.CompilerServices;

namespace TasksList.Logger
{
    public static class Log
    {
        private static bool fileLogging = false;

        /// <summary>
        /// Включает/отключает логирование в файл
        /// </summary>
        public static void EnableLogging(bool fileLog)
        {
            fileLogging = fileLog;
        }

        /// <summary>
        /// Выводит сообщение на уровень [DEBUG]
        /// </summary>
        public static void Debug(string message)
        {
            if (fileLogging)
                FileLogger.Instance.Debug(message);
        }

        /// <summary>
        /// Выводит сообщение на уровень [TRACE]
        /// </summary>
        public static void Trace(string message)
        {
            if (fileLogging)
                FileLogger.Instance.Trace(message);
        }

        /// <summary>
        /// Выводит сообщение на уровень [ERROR]
        /// </summary>
        public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string formattedMessage = FormatLogMessage(message, memberName, sourceFilePath, sourceLineNumber);

            if (fileLogging)
                FileLogger.Instance.Error(formattedMessage);
        }

        /// <summary>
        /// Выводит сообщение на уровень [FATAL]
        /// </summary>
        public static void Fatal(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string formattedMessage = FormatLogMessage(message, memberName, sourceFilePath, sourceLineNumber);

            if (fileLogging)
                FileLogger.Instance.Fatal(formattedMessage);
        }

        /// <summary>
        /// Выводит сообщение на уровень [INFO]
        /// </summary>
        public static void Info(string message)
        {
            if (fileLogging)
                FileLogger.Instance.Info(message);
        }

        /// <summary>
        /// Выводит сообщение на уровень [WARN]
        /// </summary>
        public static void Warn(string message)
        {
            if (fileLogging)
                FileLogger.Instance.Warn(message);
        }

        /// <summary>
        /// формирует строку сообщения, если была передана информация о вызывающем методе
        /// </summary>
        /// <returns>сформированное сообщение</returns>
        private static string FormatLogMessage(string message, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            return $@"{message} 
				[Caller infomation]: method  = {memberName}
				[Caller infomation]: source file = {sourceFilePath}
				[Caller infomation]: line number = {sourceLineNumber}";
        }
    }
}
