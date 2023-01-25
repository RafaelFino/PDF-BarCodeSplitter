using System.Collections.Generic;

namespace BarCodeSplitter.lib
{
    public partial class DeliveryService
    {
        public class PDFFileResult
        {
            public IList<string> Thermal { get; set; }
            public IList<string> Paper { get; set; }
        }
    }
}
