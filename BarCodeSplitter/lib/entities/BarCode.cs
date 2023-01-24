namespace BarCodeSplitter.lib
{
    public class BarCode
    {
        public string PageFile { get; set; }
        public int PageCount { get; set; }
        public string Value { get; set; }
        public string CodeType { get; set; }
        public float ElapsedTimePerPage { get; set; }

        public override string ToString()
        {
            return $"(Page: {PageCount} Type: {this.CodeType} - Value: {this.Value})";
        }
    }
}
