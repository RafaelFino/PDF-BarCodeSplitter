using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BarCodeSplitter.lib
{
    public class BarCodeSummary
    {
        public string FileSource { get; set; }
        public ConcurrentBag<BarCode> Codes { get; set; }

        public float ElaspedTime { get; set; }
        public override string ToString()
        {
            return $"File: {FileSource} Codes: [{string.Join(",", Codes)}]";
        }
    }
}
