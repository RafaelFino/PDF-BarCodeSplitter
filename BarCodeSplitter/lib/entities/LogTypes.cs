using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCodeSplitter.lib.entities
{
    public enum LogTypes
    {
        Debug = 0,
        Info = 1,   
        Warning = 2,    
        Error = 3,
        Fatal = 4,  
    }

    public class LogMsg
    {
        public LogTypes Type { get; set; }  
        public string Message { get; set; } 
    }
}
