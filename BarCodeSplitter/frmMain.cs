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
        private PDFToolKit pdftk = new PDFToolKit();
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public frmMain()
        {
            InitializeComponent();
            

            //fill file type combo box
            foreach (var item in Enum.GetValues(typeof(FileTypes)))
            {
                logMessage($"Add item {item} to FileType Combo Box");
                cmbOutputType.Items.Add(item);
            }

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

            pdftk.LogMessage += OnLog;            
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
            if (Directory.Exists(txtInput.Text))
            {
                foreach (var item in Directory.GetFiles(txtInput.Text, "*.pdf"))
                {
                    logMessage($"Reading {item} from txtInput.Text", LogTypes.Info);
                    var output = $"{txtInput.Text}\\output";
                    if (!Directory.Exists(output))
                    {
                        logMessage($"Creating {output} directory to output files", LogTypes.Debug);
                        Directory.CreateDirectory(output);
                    }
                    pdftk.Run(item, output);

                    logMessage($"Done for {item}", LogTypes.Info);
                }
            }
            else
            {
                logMessage($"{txtInput.Text} is an invalid path", LogTypes.Error);
            }

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
