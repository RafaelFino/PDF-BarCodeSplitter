using ImageMagick;
using Newtonsoft.Json;
using PdfSharp.Pdf;
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

        public BarCodeSummary Run(string filename, string outputPath)
        {
            var sw = Stopwatch.StartNew();
            var ret = new BarCodeSummary { FileSource = filename, Codes = new ConcurrentBag<BarCode>() };
            
            try
            {
                foreach (var item in Split(filename, outputPath))
                {
                    OnLogMessage($"[Run] Analysing {item.Value}");
                    var result = Recognize(item.Value, item.Key);

                    if (result != null)
                    {
                        ret.Codes.Add(result);
                    }
                };

                sw.Stop();
                ret.ElaspedTime = sw.ElapsedMilliseconds;

                CreateSummary(ret);
            }
            catch (Exception ex)
            {
                OnLogMessage($"[Run] ERROR {ex.Message}\n Stack: {ex.ToString()}");
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

                var newPage = $"{outputPath}\\{name}\\page {idx + 1}-{inputDocument.PageCount}.pdf";

                OnLogMessage($"Split: Creating page file {idx + 1} from {Path.GetFileNameWithoutExtension(filename)}");

                outputDocument.Save(newPage);

                ret.Add(idx + 1, newPage);
            }

            return ret;
        }

        private Tuple<string, string> FindBarCode(string filename)
        {
            IBarcodeReader reader = new BarcodeReader();
            {
                using (var barcodeBitmap = (Bitmap)Image.FromFile(filename))
                {
                    var result = reader.Decode(barcodeBitmap);

                    if (result != null)
                    {
                        return new Tuple<string, string>(result.BarcodeFormat.ToString(), result.Text);
                    }
                }
            }
            return null;
        }

        private BarCode Recognize(string pageFile, int page)
        {
            var sw = Stopwatch.StartNew();
            BarCode ret = null;
            var pngFile = $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(pageFile)}.png";

            using (var image = new MagickImage(pageFile, _readSettings))
            {
                image.Write(pngFile);

                var result = FindBarCode(pngFile);

                if (result != null)
                {
                    sw.Stop();
                    OnLogMessage($"[Recognize] Found codes on {pageFile}-> Code: {result.Item1} Value: {result.Item2}");
                    ret = new BarCode
                    {
                        CodeType = result.Item1,
                        Value = result.Item2,
                        PageFile = pageFile,
                        PageCount = page,
                        ElapsedTimePerPage = sw.ElapsedMilliseconds
                    };
                } 
                else
                {
                    if (File.Exists(pageFile))
                    {
                        OnLogMessage($"[Recognize] Removing PDF without BAR CODE {pageFile}");
                        File.Delete(pageFile);  
                    }
                }
            }

            if(File.Exists(pngFile))
            {
                OnLogMessage($"[Recognize] Removing temp file {pngFile}");
                File.Delete(pngFile);
            }

            return ret;
        }

        private static readonly JsonSerializerSettings _options = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private void CreateSummary(BarCodeSummary data)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented, _options);
            OnLogMessage($"[CreateSummary] Data: {jsonString}");
            File.WriteAllText($"{Path.GetDirectoryName(data.FileSource)}\\{Path.GetFileNameWithoutExtension(data.FileSource)}.json", jsonString);
        }
    }
}
