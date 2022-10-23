using NLog;
using NLog.Config;
using NLog.Targets;

using System;
using System.IO;

namespace TasksList.Logger
{
    public class FileLogger
    {
        /// <summary>
        /// Файловый логгер
        /// </summary>
       
        private static readonly Lazy<FileLogger> Inst = new Lazy<FileLogger>(() => new FileLogger());
        public static FileLogger Instance => Inst.Value;

        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static FileTarget fileTarget;
        private static string LogsPath = "Logger/";

        private FileLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            fileTarget = new FileTarget("target1")
            {
                KeepFileOpen = true
            };

            //Удаляем старый файл логов
            try
            {
                File.Delete(LogsPath + "LogFile.log");
            }
            catch { }
            config.AddTarget("file", fileTarget);
            //Создаём новый файл логов
            fileTarget.FileName = LogsPath + "LogFile.log";
            //Прописываем маску заполнения  (дата/время уровень_ошибки сообщение)
            fileTarget.Layout = @"[${date:format=HH\:mm\:ss}] [${level}] ${message}";

            //Прописываем правила ведения логов
            LoggingRule ruleW = new LoggingRule("*", LogLevel.Warn, fileTarget);
            ruleW.Final = true;

            LoggingRule ruleI = new LoggingRule("*", LogLevel.Info, fileTarget);
            ruleI.Final = true;

            LoggingRule ruleE = new LoggingRule("*", LogLevel.Error, fileTarget);
            ruleE.Final = true;

            LoggingRule ruleD = new LoggingRule("*", LogLevel.Debug, fileTarget);
            ruleD.Final = true;

            LoggingRule ruleT = new LoggingRule("*", LogLevel.Trace, fileTarget);
            ruleT.Final = true;

            LoggingRule ruleF = new LoggingRule("*", LogLevel.Fatal, fileTarget);
            ruleF.Final = true;

            config.LoggingRules.Add(ruleI);
            config.LoggingRules.Add(ruleW);
            config.LoggingRules.Add(ruleE);
            config.LoggingRules.Add(ruleD);
            config.LoggingRules.Add(ruleT);
            config.LoggingRules.Add(ruleF);

            LogManager.Configuration = config;
        }

        public void Enable(bool fileLogging)
        {
        }

        //Уровень сообщения Debug
        public void Debug(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget);
            logger.Debug(message);
        }

        //Уровень сообщения Trace
        public void Trace(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget);
            logger.Trace(message);
        }

        //Уровень сообщения Error
        public void Error(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget);
            logger.Error(message);
        }

        //Уровень сообщения Fatal
        public void Fatal(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget);
            logger.Fatal(message);
        }

        //Уровень сообщения Info
        public void Info(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget);
            logger.Info(message);
        }

        //Уровень сообщения Warn
        public void Warn(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget);
            logger.Warn(message);
        }
    }
}
