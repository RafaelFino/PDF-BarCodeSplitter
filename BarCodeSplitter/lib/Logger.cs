using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCodeSplitter.lib
{
    public class Logger
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string CFG_EnableDebug = "enableDebug";

        public event EventHandler<LogMsg> LogMessage;

        private static Logger _instance;
        public static Logger GetInstance { get
            {
                if (_instance == null)
                {
                    _instance = new Logger(); 
                }
                return _instance; 
            }
        }

        private Logger()
        {
        }        

        public void Log(string msg, LogLevel level = LogLevel.Info)
        {
            Log(new LogMsg { Message = msg, Level = level});
        }

        public void Log(LogMsg msg)
        {
            string fmtMessage;
            if (string.IsNullOrEmpty(msg.Source))
            {
                fmtMessage = msg.Message;
            }
            else
            {
                fmtMessage = $"[{msg.Source}] {msg.Message}";
            }
           
            switch (msg.Level)
            {
                case LogLevel.Debug:
                    {
                        _logger.Debug(msg.Message);
                        break;
                    }
                case LogLevel.Error:
                    {
                        _logger.Error(msg.Message);
                        break;
                    }
                default:
                    {
                        _logger.Info(msg.Message);
                        break;
                    }
            }

            LogMessage?.Invoke(this, msg);
        }
    }
}
