namespace BarCodeSplitter.lib
{
    public class BarCode
    {
        public string Value { get; set; }
        public string CodeType { get; set; }
        public float ProcessElapsedTime { get; set; }

        public override string ToString()
        {
            return $"Type: {this.CodeType}|Value: {this.Value}";
        }
    }
}
