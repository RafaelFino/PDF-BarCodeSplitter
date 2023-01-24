using BarCodeSplitter.lib;
using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarCodeSplitter
{
    public partial class frmMain : Form
    {
        const string CFG_LastDir = "lastDir";
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public frmMain()
        {
            InitializeComponent();

            //fill file type combo box
            cmbOutputType.DataSource = Enum.GetValues(typeof(FileTypes));
            cmbOutputType.SelectedIndex = 0;

            var lastDir = ConfigurationManager.AppSettings.Get(CFG_LastDir);

            if (string.IsNullOrEmpty(lastDir))
            {
                txtInput.Text = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                txtInput.Text = lastDir;
            }           
        }

        private void OnLog(object sender, string message)
        {
            logMessage(message, LogTypes.Debug);
        }

        private void logMessage(string message, LogTypes logType = LogTypes.Info)
        {
            var fmtMsg = $"[{DateTime.Now}] [{logType}] {message}{Environment.NewLine}";
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((MethodInvoker)delegate
                {
                    txtLog.AppendText(fmtMsg);
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.SelectionLength = 0;
                });
            }
            else
            {
                txtLog.AppendText(fmtMsg);
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.SelectionLength = 0;
            }

            switch (logType)
            {
                case LogTypes.Debug:
                    {
                        _logger.Debug(message);
                        break;
                    }
                case LogTypes.Error:
                    {
                        _logger.Error(message);
                        break;
                    }
                default:
                    {
                        _logger.Info(message);
                        break;
                    }
            }
        }

        private void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                if (settings[key] == null)
                {
                    settings.Add(key, value);
                    logMessage($"App.Config updated: NewValue [{key}:{value}]", LogTypes.Debug);
                }
                else
                {
                    if (settings[key].Value != value)
                    {
                        settings[key].Value = value;
                        logMessage($"App.Config updated: [{key}:{value}]", LogTypes.Debug);
                    }
                    else
                    {
                        return;
                    }
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                logMessage($"App.Config updated [{key}:{value}]", LogTypes.Debug);
            }
            catch (ConfigurationErrorsException)
            {
                logMessage($"Error writing app settings [{key}:{value}]", LogTypes.Error);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            var pdftk = new PDFToolKit();
            pdftk.LogMessage += OnLog;

            if (Directory.Exists(txtInput.Text))
            {
                foreach (var item in Directory.GetFiles(txtInput.Text, "*.pdf"))
                {
                    FileTypes fileType;
                    Enum.TryParse<FileTypes>(cmbOutputType.SelectedValue.ToString(), out fileType);

                    RunFile(pdftk, item, fileType);
                }
            }
            else
            {
                logMessage($"{txtInput.Text} is an invalid path", LogTypes.Error);
            }
        }

        private PDFFile RunFile(PDFToolKit pdftk, string item, FileTypes fileType)
        {
            logMessage($"Reading {item} from txtInput.Text - Using {fileType}", LogTypes.Info);

            var output = $"{txtInput.Text}\\output";
            if (!Directory.Exists(output))
            {
                logMessage($"Creating {output} directory to output files", LogTypes.Debug);
                Directory.CreateDirectory(output);
            }

            var result = pdftk.Analyze(item, output, CreateConfig(fileType));

            ProcessResult(result, fileType, $"{output}\\{fileType}");

            logMessage($"Done for {item}", LogTypes.Info);

            return result;
        }

        private void ProcessResult(PDFFile result, FileTypes fileType, string output)
        {
            var outputThermal = $"{output}\\thermal";
            if (!Directory.Exists(outputThermal))
            {
                logMessage($"Creating {outputThermal} directory to output thermal files", LogTypes.Debug);
                Directory.CreateDirectory(outputThermal);
            }

            var outputPaper = $"{output}\\paper";
            if (!Directory.Exists(outputPaper))
            {
                logMessage($"Creating {outputPaper} directory to output paper files", LogTypes.Debug);
                Directory.CreateDirectory(outputPaper);
            }

            switch (fileType)
            {
                case FileTypes.FedEx:
                    {
                        foreach (var page in result.Pages)
                            ProcessFedEX(fileType, outputThermal, outputPaper, page);
                        break;
                    }
                case FileTypes.UPS:
                    {
                        foreach (var page in result.Pages)
                            ProcessUPS(fileType, outputThermal, outputPaper, page);
                        break;
                    }
            }
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

        private void ProcessUPS(FileTypes fileType, string outputThermal, string outputPaper, PDFPage page)
        {
            if (!string.Join("", page.Text).ToUpper().Contains("INVOICE"))
            {
                logMessage($"[{fileType}] {page.PageFile} is THERMAL", LogTypes.Debug);
                MovePDF(page.PageFile, $"{outputThermal}\\{Path.GetFileName(page.PageFile)}", false, fileType);
            }
            else
            {
                logMessage($"[{fileType}] {page.PageFile} is PAPER", LogTypes.Debug);
                MovePDF(page.PageFile, $"{outputPaper}\\{Path.GetFileName(page.PageFile)}", false, fileType);
            }
        }

        private void ProcessFedEX(FileTypes fileType, string outputThermal, string outputPaper, PDFPage page)
        {
            if (page.Code != null)
            {
                logMessage($"[{fileType}] {page.PageFile} is THERMAL", LogTypes.Debug);
                MovePDF(page.PageFile, $"{outputThermal}\\{Path.GetFileName(page.PageFile)}", false, fileType);
            }
            else
            {
                logMessage($"[{fileType}] {page.PageFile} is PAPER", LogTypes.Debug);
                MovePDF(page.PageFile, $"{outputPaper}\\{Path.GetFileName(page.PageFile)}", false, fileType);
            }
        }

        private void MovePDF(string source, string destiny, bool overridde, FileTypes fileType)
        {
            if (File.Exists(destiny))
            {
                if (overridde)
                {
                    logMessage($"[{fileType}] {destiny} already exists, ignoring", LogTypes.Info);
                    return;
                }
                File.Delete(destiny);
            }

            logMessage($"[{fileType}] Delivering {Path.GetFileName(source)}  to {destiny}", LogTypes.Info);
            File.Move(source, destiny);
        }

        private void txtInput_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog() { SelectedPath = txtInput.Text })
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtInput.Text = fbd.SelectedPath;
                    AddUpdateAppSettings(CFG_LastDir, fbd.SelectedPath);
                }
            }
        }
    }
}
