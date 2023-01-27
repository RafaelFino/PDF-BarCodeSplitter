using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BarCodeSplitter.lib
{
    public partial class DeliveryService
    {
        private PDFToolKit _pdftk = new PDFToolKit();

        public PDFToolKit Pdftk { get => _pdftk; }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            Logger.GetInstance.Log(new LogMsg() { Source = "Delivery", Message = message, Level = level });
        }

        private readonly HashSet<TaskStatus> _runningStatus = new HashSet<TaskStatus>() { TaskStatus.Running, TaskStatus.WaitingToRun };

        public void RunBatch(string input, FileTypes fileType)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                Log($"Batch start for {input}", LogLevel.Info);

                if (Directory.Exists(input))
                {
                    var results = new ConcurrentBag<Task>();
                    var files = Directory.GetFiles(input, "*.pdf");
                    Parallel.ForEach(files, item =>
                    {
                        Log($"[{fileType}] Creating new Thread for to process {item} - {results.Count + 1}/{files.Length}", LogLevel.Info);
                        results.Add(Task.Factory.StartNew(() => RunFile(input, item, fileType)));
                    });

                    //wait to start to show batch log
                    Thread.Sleep(TimeSpan.FromSeconds(files.Length));

                    while (results.Where(t => _runningStatus.Contains(t.Status)).Any())
                    {
                        var running = results.Where(t => _runningStatus.Contains(t.Status)).Count();
                        Log($"[{fileType}] Batch still running for {running}/{files.Length} Threads yet, please wait...", LogLevel.Info);

                        //2 seconds per thread
                        Thread.Sleep(TimeSpan.FromSeconds(Math.Max(running*2, 5)));
                    }                    
                }
                else
                {
                    Log($"{input} is an invalid path", LogLevel.Error);
                }

                sw.Stop();  
                Log($"[{fileType}] Batch process finished for {input} in {sw.ElapsedMilliseconds} ms", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.GetInstance.Log(new LogMsg { Source = "Delivery", Message = $"Error: {ex.ToString()}", Level = LogLevel.Error });
            }
        }

        public PDFFile RunFile(string input, string item, FileTypes fileType)
        {
            try
            {
                var output = $"{input}\\output";
                if (!Directory.Exists(output))
                {
                    Log($"Creating {output} directory to output files", LogLevel.Debug);
                    Directory.CreateDirectory(output);
                }

                var result = _pdftk.Analyze(item, output, CreateConfig(fileType));

                ProcessResult(result, fileType, $"{output}\\{fileType}");

                Log($"[{fileType}] Done for {item}", LogLevel.Info);

                return result;
            }
            catch (Exception ex)
            {
                Logger.GetInstance.Log(new LogMsg { Source = "Delivery", Message = $"Error: {ex.ToString()}", Level = LogLevel.Error });
                throw;
            }
        }

        private void ProcessResult(PDFFile file, FileTypes fileType, string output)
        {
            var outputThermalPath = $"{output}\\thermal";
            if (!Directory.Exists(outputThermalPath))
            {
                Log($"Creating {outputThermalPath} directory to output thermal files", LogLevel.Debug);
                Directory.CreateDirectory(outputThermalPath);
            }

            var outputPaperPath = $"{output}\\paper";
            if (!Directory.Exists(outputPaperPath))
            {
                Log($"Creating {outputPaperPath} directory to output paper files", LogLevel.Debug);
                Directory.CreateDirectory(outputPaperPath);
            }

            PDFFileResult result = null;

            switch (fileType)
            {
                case FileTypes.FedEx:
                    {
                        result = ProcessFedEX(file);
                        break;
                    }
                case FileTypes.UPS:
                    {
                        result = ProcessUPS(file);
                        break;
                    }
            }

            var outputThermalFile = $"{outputThermalPath}\\{Path.GetFileName(file.FileSource)}";
            Log($"[{fileType}] Delivering THREMAL {outputThermalFile}", LogLevel.Info);
            _pdftk.Concat(result.Thermal, outputThermalFile);

            var outputPaperFile = $"{outputPaperPath}\\{Path.GetFileName(file.FileSource)}";
            Log($"[{fileType}] Delivering PAPER {outputPaperFile}", LogLevel.Info);
            _pdftk.Concat(result.Paper, outputPaperFile);
        }

        private PDFAnalyzeConfig CreateConfig(FileTypes fileType)
        {
            var config = new PDFAnalyzeConfig() { MakeJsonReport = true };

            switch (fileType)
            {
                case FileTypes.FedEx:
                    {
                        config.FindBarCode = true;
                        config.GetAllText = false;
                        break;
                    }
                case FileTypes.UPS:
                    {
                        config.FindBarCode = false;
                        config.GetAllText = true;
                        break;
                    }
            }

            return config;
        }

        private PDFFileResult ProcessUPS(PDFFile file)
        {
            var ret = new PDFFileResult() { Paper = new List<string>(), Thermal = new List<string>() };

            foreach (var page in file.Pages)
            {
                var data = string.Join("", page.Text).ToUpper();
                if (!data.Contains("INVOICE"))
                {
                    Log($"[{FileTypes.UPS}] {page.PageFile} is THERMAL", LogLevel.Debug);
                    ret.Thermal.Add(page.PageFile);
                }
                else
                {
                    Log($"[{FileTypes.UPS}] {page.PageFile} is PAPER", LogLevel.Debug);
                    ret.Paper.Add(page.PageFile);
                }
            }

            return ret;
        }

        private PDFFileResult ProcessFedEX(PDFFile file)
        {
            var ret = new PDFFileResult() { Paper = new List<string>(), Thermal = new List<string>() };

            foreach (var page in file.Pages)
            {
                if (page.Code != null && page.Code.CodeType == "PDF_417")
                {
                    Log($"[{FileTypes.UPS}] {page.PageFile} is THERMAL", LogLevel.Debug);
                    ret.Thermal.Add(page.PageFile);
                }
                else
                {
                    Log($"[{FileTypes.UPS}] {page.PageFile} is PAPER", LogLevel.Debug);
                    ret.Paper.Add(page.PageFile);
                }
            }

            return ret;
        }
    }
}
