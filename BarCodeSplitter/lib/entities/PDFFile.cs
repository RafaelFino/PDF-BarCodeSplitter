using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BarCodeSplitter.lib
{
    public class PDFFile
    {
        public string FileSource { get; set; }
        public ConcurrentBag<PDFPage> Pages { get; set; }

        public float ProcessElaspedTime { get; set; }
        public override string ToString()
        {
            return $"File: {FileSource}|Codes: [{string.Join(",", Pages)}]";
        }

        public IEnumerable<string> Text
        {
            get
            {
                var ret = new List<string>();
                if (Pages != null)
                {
                    foreach (var page in Pages)
                    {
                        ret.Add(page.Text);
                    }
                }
                return ret;
            }
        }
    }
}
