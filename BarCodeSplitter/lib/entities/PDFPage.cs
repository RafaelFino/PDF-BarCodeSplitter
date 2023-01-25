using System.Collections.Generic;

namespace BarCodeSplitter.lib
{
    public class PDFPage
    {
        public BarCode Code { get; set; }
        public int PageNumber { get; set; }
        public string PageFile { get; set; }
        public string Text { get; set; }
        public float ProcessElaspedTime { get; set; }

        public override string ToString()
        {
            return $"File: {PageNumber}|Code: {Code}|Text: {Text}";
        }
    }
}
