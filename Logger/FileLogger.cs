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
       
        private static readonly Lazy<FileLogger> _inst = new Lazy<FileLogger>(() => new FileLogger());
        public static FileLogger _instance => _inst.Value;

        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        private static FileTarget _fileTarget;
        private static string _logsPath = "Logger/";

        private FileLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            _fileTarget = new FileTarget("target1")
            {
                KeepFileOpen = true
            };

            //Удаляем старый файл логов
            try
            {
                File.Delete(_logsPath + "LogFile.log");
            }
            catch { }
            config.AddTarget("file", _fileTarget);
            //Создаём новый файл логов
            _fileTarget.FileName = _logsPath + "LogFile.log";
            //Прописываем маску заполнения  (дата/время уровень_ошибки сообщение)
            _fileTarget.Layout = @"[${date:format=HH\:mm\:ss}] [${level}] ${message}";

            //Прописываем правила ведения логов
            LoggingRule ruleW = new LoggingRule("*", LogLevel.Warn, _fileTarget);
            ruleW.Final = true;

            LoggingRule ruleI = new LoggingRule("*", LogLevel.Info, _fileTarget);
            ruleI.Final = true;

            LoggingRule ruleE = new LoggingRule("*", LogLevel.Error, _fileTarget);
            ruleE.Final = true;

            LoggingRule ruleD = new LoggingRule("*", LogLevel.Debug, _fileTarget);
            ruleD.Final = true;

            LoggingRule ruleT = new LoggingRule("*", LogLevel.Trace, _fileTarget);
            ruleT.Final = true;

            LoggingRule ruleF = new LoggingRule("*", LogLevel.Fatal, _fileTarget);
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
            SimpleConfigurator.ConfigureForTargetLogging(_fileTarget);
            _logger.Debug(message);
        }

        //Уровень сообщения Trace
        public void Trace(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(_fileTarget);
            _logger.Trace(message);
        }

        //Уровень сообщения Error
        public void Error(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(_fileTarget);
            _logger.Error(message);
        }

        //Уровень сообщения Fatal
        public void Fatal(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(_fileTarget);
            _logger.Fatal(message);
        }

        //Уровень сообщения Info
        public void Info(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(_fileTarget);
            _logger.Info(message);
        }

        //Уровень сообщения Warn
        public void Warn(string message)
        {
            SimpleConfigurator.ConfigureForTargetLogging(_fileTarget);
            _logger.Warn(message);
        }
    }
}
