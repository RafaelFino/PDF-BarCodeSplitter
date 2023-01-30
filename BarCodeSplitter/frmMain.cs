using BarCodeSplitter.lib;
using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarCodeSplitter
{
    public partial class frmMain : Form
    {
        const string CFG_LastDir = "lastDir";

        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DeliveryService _delivery = new DeliveryService();
        private const int MAX_LOG_LINES = 500;        

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

            Logger.GetInstance.LogMessage += OnLog;
        }

        private void OnLog(object sender, LogMsg msg)
        {
            if (msg.Level == LogLevel.Debug)
            {
                return;
            }

            var fmtMsg = $"[{DateTime.Now}] [{msg.Level}] [{msg.Source}] {msg.Message}";
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((MethodInvoker)delegate
                {
                    WriteLog(fmtMsg);
                });
            }
            else
            {
                WriteLog(fmtMsg);
            }
        }

        private void WriteLog(string message)
        {
            if (txtLog.Lines.Length >= MAX_LOG_LINES - 100)
            {
                txtLog.Lines = txtLog.Lines.Skip(100).ToArray();
            }

            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.SelectionLength = 0;
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
                    Logger.GetInstance.Log($"App.Config updated: NewValue [{key}:{value}]", LogLevel.Debug);
                }
                else
                {
                    if (settings[key].Value != value)
                    {
                        settings[key].Value = value;
                    }
                    else
                    {
                        return;
                    }
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                Logger.GetInstance.Log($"App.Config updated [{key}:{value}]", LogLevel.Debug);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.GetInstance.Log($"Error writing app settings [{key}:{value}]", LogLevel.Error);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            FileTypes fileType;
            Enum.TryParse<FileTypes>(cmbOutputType.SelectedValue.ToString(), out fileType);

            Task.Factory.StartNew(() => _delivery.RunBatch(txtInput.Text, fileType));
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

        private void btnLog_Click(object sender, EventArgs e)
        {
            frmLog log = new frmLog();
            log.ShowDialog();   
        }
    }
}
