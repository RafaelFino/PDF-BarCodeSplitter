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
        protected virtual void Log(string message, LogLevel level = LogLevel.Debug)
        {
            Logger.GetInstance.Log(new LogMsg { Source = "PDFToolKit", Message = message, Level = level });
        }
        public PDFFile Analyze(string filename, string outputPath, PDFAnalyzeConfig config)
        {
            var sw = Stopwatch.StartNew();
            var ret = new PDFFile { FileSource = filename };
            var pages = new List<PDFPage>();

            try
            {
                StatusManager.GetInstance.UpdateStatus(filename, ItemStatus.Splitting);
                var files = Split(filename, $"{Path.GetTempPath()}PDFBarCodeSplitter");

                if (files.Count == 0)
                {
                    Log($"[Analyze] No pages splitted on {filename}", LogLevel.Error);
                }

                foreach (var item in files)
                {
                    var page = new PDFPage { PageFile = item.Value, PageNumber = item.Key };

                    if (config.FindBarCode)
                    {
                        StatusManager.GetInstance.UpdateStatus(filename, ItemStatus.SearchingBarCode);
                        Log($"[Analyze] Searching for a bar code {page.PageFile}");
                        page.Code = FindBarCode(page.PageFile, page.PageNumber);
                    }

                    if (config.GetAllText)
                    {
                        StatusManager.GetInstance.UpdateStatus(filename, ItemStatus.SearchingText);
                        Log($"[Analyze] Trying to extractg text from {page.PageFile}");
                        page.Text = GetText(page.PageFile);
                    }

                    pages.Add(page);
                }

                Log($"[Analyze] Creating JSON Report for {filename}");
                CreateSummary(ret, outputPath, config.MakeJsonReport);

                sw.Stop();
                ret.ProcessElaspedTime = sw.ElapsedMilliseconds;
                ret.Pages = new ConcurrentBag<PDFPage>(pages);  
            }
            catch (Exception ex)
            {
                Log($"[Analyze] ERROR {ex.Message}\n Details: {ex}");
                throw;
            }

            return ret;
        }

        public int CountPages(string filename)
        {
            var ret = 0;

            using (PdfDocument inputDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import))
            {
                ret = inputDocument.PageCount;
            }

            return ret;
        }

        private Dictionary<int, string> Split(string filename, string outputPath)
        {
            var ret = new Dictionary<int, string>();
            Log($"[Split] Open {filename}");

            using (PdfDocument inputDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import))
            {
                if (!Directory.Exists(outputPath))
                {                    
                    Directory.CreateDirectory(outputPath);
                }

                string name = Path.GetFileNameWithoutExtension(filename);

                var pdfPath = $"{outputPath}\\{name}";

                if (inputDocument.PageCount == 0)
                {
                    Log($"[Split] File {filename} doesn't have pages!", LogLevel.Error);
                }

                if (Directory.Exists(pdfPath))
                {
                    Directory.Delete(pdfPath, true);
                }

                if (!Directory.Exists(pdfPath))
                {
                    Directory.CreateDirectory(pdfPath);
                }

                StatusManager.GetInstance.UpdatePages(filename, inputDocument.PageCount);

                for (int idx = 0; idx < inputDocument.PageCount; idx++)
                {
                    PdfDocument outputDocument = CreateDocument();
                    outputDocument.Version = inputDocument.Version;
                    outputDocument.Info.Title = $"Page {idx + 1}/{inputDocument.PageCount} of {inputDocument.Info.Title}";
                    outputDocument.Info.Creator = inputDocument.Info.Creator;

                    outputDocument.AddPage(inputDocument.Pages[idx]);

                    var newPage = $"{pdfPath}\\pg {idx + 1}-{inputDocument.PageCount}.pdf";

                    Log($"[Split] Creating page file {idx + 1} from {Path.GetFileNameWithoutExtension(filename)}");

                    outputDocument.Save(newPage);

                    if (!File.Exists(newPage))
                    {
                        Log($"[Split] Fail to create PDF Page {newPage}", LogLevel.Error);
                    }

                    ret.Add(idx + 1, newPage);
                }
            }

            return ret;
        }

        private PdfDocument CreateDocument()
        {
            var document = new PdfDocument();

            document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
            document.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;
            document.Options.NoCompression = false;
            document.Options.CompressContentStreams = true;

            return document;
        }        

        private BarCode FindBarCode(string pageFile, int page)
        {
            var sw = Stopwatch.StartNew();
            BarCode ret = null;
            var pngFile = $"{pageFile}.png";

            if (!File.Exists(pageFile))
            {
                Log($"[FindBarCode] Fail to open PDF Page {pageFile}", LogLevel.Error);
                throw new FileNotFoundException(pageFile);
            }

            if (File.Exists(pngFile))
            {
                Log($"[FindBarCode] PNG temp file {pngFile} already exists, deleting before write a new", LogLevel.Error);
                File.Delete(pngFile);
            }            

            using (var image = new MagickImage(pageFile, _readSettings))
            {
                image.Write(pngFile);

                if (!File.Exists(pngFile))
                {
                    Log($"[FindBarCode] Fail to write and open PNG file {pngFile}", LogLevel.Error);
                    throw new FileNotFoundException(pngFile);
                }

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

        private void CreateSummary(PDFFile data, string outputPath, bool writeJsonReport = false)
        {
            StatusManager.GetInstance.UpdateStatus(data.FileSource, ItemStatus.CreatingSummary);
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented, _options);
            Log($"[CreateSummary] Data: {jsonString}");

            if (writeJsonReport)
            {
                File.WriteAllText($"{outputPath}\\{Path.GetFileNameWithoutExtension(data.FileSource)}.json", jsonString);
            }            
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

        public void Concat(IList<string> files, string outputFile, bool deleteSource = false)
        {
            if (files == null || files.Count == 0)
            {
                return;
            }

            using (var outputDocument = new PdfDocument())
            {
                foreach (string file in files)
                {
                    using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                    {
                        foreach (var page in inputDocument.Pages)
                        {
                            outputDocument.AddPage(page);
                        }
                    }

                    if (deleteSource)
                    {
                        File.Delete(file);
                    }
                }

                outputDocument.Save(outputFile);
            }
        }
    }
}
