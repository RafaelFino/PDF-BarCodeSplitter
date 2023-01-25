using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Generic;
using System.IO;

namespace BarCodeSplitter.lib
{
    public partial class DeliveryService
    {
        public event EventHandler<LogMsg> LogMessage;
        private PDFToolKit _pdftk = new PDFToolKit();

        public PDFToolKit Pdftk { get => _pdftk; }

        public void Log(string message, LogTypes logType = LogTypes.Info)
        {
            LogMessage?.Invoke(this, new LogMsg() { Message = $"[Delivery] {message}", Type = logType });
        }

        public PDFFile RunFile(string input, string item, FileTypes fileType)
        {
            Log($"Reading {item} from txtInput.Text - Using {fileType}", LogTypes.Info);

            var output = $"{input}\\output";
            if (!Directory.Exists(output))
            {
                Log($"Creating {output} directory to output files", LogTypes.Debug);
                Directory.CreateDirectory(output);
            }

            var result = _pdftk.Analyze(item, output, CreateConfig(fileType));

            ProcessResult(result, fileType, $"{output}\\{fileType}");

            Log($"Done for {item}", LogTypes.Info);

            return result;
        }

        private void ProcessResult(PDFFile file, FileTypes fileType, string output)
        {
            var outputThermalPath = $"{output}\\thermal";
            if (!Directory.Exists(outputThermalPath))
            {
                Log($"Creating {outputThermalPath} directory to output thermal files", LogTypes.Debug);
                Directory.CreateDirectory(outputThermalPath);
            }

            var outputPaperPath = $"{output}\\paper";
            if (!Directory.Exists(outputPaperPath))
            {
                Log($"Creating {outputPaperPath} directory to output paper files", LogTypes.Debug);
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
            Log($"[{FileTypes.UPS}] Delivering THREMAL {outputThermalFile}", LogTypes.Info);
            _pdftk.Concat(result.Thermal, outputThermalFile);

            var outputPaperFile = $"{outputPaperPath}\\{Path.GetFileName(file.FileSource)}";
            Log($"[{FileTypes.UPS}] Delivering PAPER {outputPaperFile}", LogTypes.Info);
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
                    Log($"[{FileTypes.UPS}] {page.PageFile} is THERMAL", LogTypes.Debug);
                    ret.Thermal.Add(page.PageFile);
                }
                else
                {
                    Log($"[{FileTypes.UPS}] {page.PageFile} is PAPER", LogTypes.Debug);
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
                    Log($"[{FileTypes.UPS}] {page.PageFile} is THERMAL", LogTypes.Debug);
                    ret.Thermal.Add(page.PageFile);
                }
                else
                {
                    Log($"[{FileTypes.UPS}] {page.PageFile} is PAPER", LogTypes.Debug);
                    ret.Paper.Add(page.PageFile);
                }
            }

            return ret;
        }
    }
}
