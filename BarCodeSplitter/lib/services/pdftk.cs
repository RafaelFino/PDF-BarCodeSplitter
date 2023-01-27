using BarCodeSplitter.lib.entities;
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
using UglyToad.PdfPig.Content;
using ZXing;

namespace BarCodeSplitter.lib
{
    public partial class PDFToolKit
    {         
        private MagickReadSettings _readSettings = new MagickReadSettings() { Density = new Density(300) };

        protected virtual void Log(LogMsg msg)
        {
            Logger.GetInstance.Log(msg);
        }
        protected virtual void Log(string message)
        {
            Logger.GetInstance.Log(new LogMsg { Source = "PDFToolKit", Message = message, Level = LogLevel.Debug });
        }
        public PDFFile Analyze(string filename, string outputPath, PDFAnalyzeConfig config)
        {
            var sw = Stopwatch.StartNew();
            var ret = new PDFFile { FileSource = filename, Pages = new ConcurrentBag<PDFPage>() };

            try
            {
                foreach (var item in Split(filename, $"{Path.GetTempPath()}PDFBarCodeSplitter"))
                {
                    var page = new PDFPage() { PageFile = item.Value, PageNumber = item.Key };

                    if (config.FindBarCode)
                    {
                        Log($"[Analyze] Searching for a bar code {page.PageFile}");
                        page.Code = FindBarCode(page.PageFile, page.PageNumber);
                    }

                    if (config.GetAllText)
                    {
                        Log($"[Analyze] Trying to extractg text from {page.PageFile}");
                        page.Text = GetText(page.PageFile);
                    }

                    ret.Pages.Add(page);
                };

                if (config.MakeJsonReport)
                {
                    Log($"[Analyze] Creating JSON Report for {filename}");
                    CreateSummary(ret, outputPath);
                }

                sw.Stop();
                ret.ProcessElaspedTime = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                Log($"[Analyze] ERROR {ex.Message}\n Stack: {ex.ToString()}");
            }

            return ret;
        }

        private Dictionary<int, string> Split(string filename, string outputPath)
        {
            var ret = new Dictionary<int, string>();
            Log($"[Split] Open {filename}");

            PdfDocument inputDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string name = Path.GetFileNameWithoutExtension(filename);
            for (int idx = 0; idx < inputDocument.PageCount; idx++)
            {
                PdfDocument outputDocument = new PdfDocument();
                outputDocument.Version = inputDocument.Version;
                outputDocument.Info.Title =
                  String.Format("Page {0} of {1}", idx + 1, inputDocument.Info.Title);
                outputDocument.Info.Creator = inputDocument.Info.Creator;

                outputDocument.AddPage(inputDocument.Pages[idx]);
                if (!Directory.Exists($"{outputPath}\\{name}"))
                {
                    Directory.CreateDirectory($"{outputPath}\\{name}");
                }

                var newPage = $"{outputPath}\\{name}\\pg {idx + 1}-{inputDocument.PageCount}.pdf";

                Log($"[Split] Creating page file {idx + 1} from {Path.GetFileNameWithoutExtension(filename)}");

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

                        Log($"[FindBarCode] Found codes on {pageFile}-> Code: {ret.CodeType} Value: {ret.Value}");
                    }
                }

                if (File.Exists(pngFile))
                {
                    Log($"[FindBarCode] Removing temp file {pngFile}");
                    File.Delete(pngFile);
                }
            }

            return ret;
        }

        private static readonly JsonSerializerSettings _options = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private void CreateSummary(PDFFile data, string outputPath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented, _options);
            Log($"[CreateSummary] Data: {jsonString}");
            File.WriteAllText($"{outputPath}\\{Path.GetFileNameWithoutExtension(data.FileSource)}.json", jsonString);
        }
        private string GetText(string filename)
        {
            var ret = new List<string>();
            using (var document = UglyToad.PdfPig.PdfDocument.Open(filename))
            {
                foreach (Page page in document.GetPages())
                {
                    foreach (var item in page.Letters)
                    {
                        ret.Add(item.Value);
                    }
                }
            }

            return string.Join(string.Empty, ret);
        }

        public void Concat(IList<string> files, string outputFile)
        {
            if (files == null || files.Count == 0)
            {
                return;
            }

            var outputDocument = new PdfDocument();

            foreach (string file in files)
            {
                PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                foreach (var page in inputDocument.Pages)
                {
                    outputDocument.AddPage(page);
                }
            }

            outputDocument.Save(outputFile);
        }
    }
}
