using ImageMagick;
using Newtonsoft.Json;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using ZXing;

namespace BarCodeSplitter.lib
{
    public class PDFToolKit
    {
        public event EventHandler<string> LogMessage;
        private MagickReadSettings _readSettings = new MagickReadSettings() { Density = new Density(300) };

        protected virtual void OnLogMessage(string message)
        {
            LogMessage?.Invoke(this, $"[PDFToolKit] {message}");
        }

        public PDFFile Analyze(string filename, string outputPath, PDFAnalyzeConfig config)
        {
            var sw = Stopwatch.StartNew();
            var ret = new PDFFile { FileSource = filename, Pages = new ConcurrentBag<PDFPage>() };

            try
            {
                foreach (var item in Split(filename, $"{outputPath}\\tmp"))
                {
                    var page = new PDFPage() {  PageFile = item.Value, PageNumber = item.Key };

                    if (config.FindBarCode)
                    {
                        OnLogMessage($"[Analyze] Searching for a bar code {page.PageFile}");
                        page.Code = FindBarCode(page.PageFile, page.PageNumber);
                    }

                    if (config.GetAllText)
                    {
                        OnLogMessage($"[Analyze] Trying to extractg text from {page.PageFile}");
                        page.Text = ExtractText(page.PageFile);
                    }

                    ret.Pages.Add(page);    
                };

                if (config.MakeJsonReport)
                {
                    OnLogMessage($"[Analyze] Creating JSON Report for {filename}");
                    CreateSummary(ret, outputPath);
                }

                sw.Stop();
                ret.ProcessElaspedTime = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                OnLogMessage($"[Analyze] ERROR {ex.Message}\n Stack: {ex.ToString()}");
            }

            return ret;
        }

        private Dictionary<int, string> Split(string filename, string outputPath)
        {
            var ret = new Dictionary<int, string>();
            // Open the file
            OnLogMessage($"[Split] Open {filename}");

            PdfDocument inputDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string name = Path.GetFileNameWithoutExtension(filename);
            for (int idx = 0; idx < inputDocument.PageCount; idx++)
            {
                // Create new document
                PdfDocument outputDocument = new PdfDocument();
                outputDocument.Version = inputDocument.Version;
                outputDocument.Info.Title =
                  String.Format("Page {0} of {1}", idx + 1, inputDocument.Info.Title);
                outputDocument.Info.Creator = inputDocument.Info.Creator;

                // Add the page and save it
                outputDocument.AddPage(inputDocument.Pages[idx]);
                if (!Directory.Exists($"{outputPath}\\{name}"))
                {
                    Directory.CreateDirectory($"{outputPath}\\{name}");
                }

                var newPage = $"{outputPath}\\{name}\\{name} - page {idx + 1}-{inputDocument.PageCount}.pdf";

                OnLogMessage($"[Split] Creating page file {idx + 1} from {Path.GetFileNameWithoutExtension(filename)}");

                outputDocument.Save(newPage);

                ret.Add(idx + 1, newPage);
            }

            return ret;
        }

        private BarCode FindBarCode(string pageFile, int page)
        {
            var sw = Stopwatch.StartNew();
            BarCode ret = null;
            var pngFile = $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(pageFile)}.png";

            using (var image = new MagickImage(pageFile, _readSettings))
            {
                image.Write(pngFile);

                IBarcodeReader reader = new BarcodeReader();
                using (var barcodeBitmap = (Bitmap)Image.FromFile(pngFile))
                {
                    var result = reader.Decode(barcodeBitmap);

                    if (result != null)
                    {
                        sw.Stop();
                        ret = new BarCode
                        {
                            CodeType = result.BarcodeFormat.ToString(),
                            Value = result.Text,
                            ProcessElapsedTime = sw.ElapsedMilliseconds
                        };

                        OnLogMessage($"[FindBarCode] Found codes on {pageFile}-> Code: {ret.CodeType} Value: {ret.Value}");
                    }
                }

                if (File.Exists(pngFile))
                {
                    OnLogMessage($"[FindBarCode] Removing temp file {pngFile}");
                    File.Delete(pngFile);
                }
            }

            return ret;
        }

        private static readonly JsonSerializerSettings _options = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private void CreateSummary(PDFFile data, string outputPath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented, _options);
            OnLogMessage($"[CreateSummary] Data: {jsonString}");
            File.WriteAllText($"{Path.GetDirectoryName(outputPath)}\\{Path.GetFileNameWithoutExtension(data.FileSource)}.json", jsonString);
        }

        private IEnumerable<string> ExtractText(string filename)
        {
            var ret = new List<string>();
            PdfDocument doc = PdfReader.Open(filename, PdfDocumentOpenMode.Import);

            foreach (var page in doc.Pages)
            {
                ret.AddRange(ExtractText(page));
            }

            return ret;
        }

        private IEnumerable<string> ExtractText(PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = ExtractText(content);
            return text;
        }

        private IEnumerable<string> ExtractText(CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                        foreach (var txt in ExtractText(cOperand))
                            yield return txt;
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                    foreach (var txt in ExtractText(element))
                        yield return txt;
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return cString.Value;
            }
        }
    }
}
