using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BarCodeSplitter.lib
{
    public class PDFPage
    {
        public BarCode Code { get; set; }
        public int PageNumber { get; set; }
        public string PageFile { get; set; }

        public string PNGFile { get; set; } 
        public string Text { get; set; }
        public float ProcessElaspedTime { get; set; }

        public string Hash { get; set; }    

        public override string ToString()
        {
            return $"File: {PageFile}:{PageNumber}|Code: {Code}|Text: {Text}";
        }
    }
}
