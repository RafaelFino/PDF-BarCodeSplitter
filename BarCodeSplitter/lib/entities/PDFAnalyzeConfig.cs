namespace BarCodeSplitter.lib
{
    public class PDFAnalyzeConfig
    {
        public bool FindBarCode { get; set; }
        public bool GetAllText { get; set; }
        public bool MakeJsonReport { get; set; } = false;

        public bool CreateHash { get; set; } = false;
    }
}
