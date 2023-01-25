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
        const string CFG_EnableDebug = "enableDebug";

        private bool _enableDebug = true;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DeliveryService _delivery = new DeliveryService();

        public frmMain()
        {
            InitializeComponent();

            //fill file type combo box
            cmbOutputType.DataSource = Enum.GetValues(typeof(FileTypes));
            cmbOutputType.SelectedIndex = 0;

            if (ConfigurationManager.AppSettings.AllKeys.Contains(CFG_EnableDebug))
            {
                var enableDebugValue = ConfigurationManager.AppSettings.Get(CFG_EnableDebug);

                _enableDebug = Convert.ToBoolean(enableDebugValue);
            }


            var lastDir = ConfigurationManager.AppSettings.Get(CFG_LastDir);

            if (string.IsNullOrEmpty(lastDir))
            {
                txtInput.Text = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                txtInput.Text = lastDir;
            }

            _delivery.Pdftk.LogMessage += OnLog;
            _delivery.LogMessage += OnLog;
        }

        private void OnLog(object sender, LogMsg msg)
        {
            LogMessage(msg.Message, msg.Type);
        }
        private void OnLog(object sender, string message)
        {
            LogMessage(message, LogTypes.Debug);
        }

        private void LogMessage(string message, LogTypes logType = LogTypes.Info)
        {
            if (!_enableDebug && logType == LogTypes.Debug)
            {
                return;
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
                    LogMessage($"App.Config updated: NewValue [{key}:{value}]", LogTypes.Debug);
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
                LogMessage($"App.Config updated [{key}:{value}]", LogTypes.Debug);
            }
            catch (ConfigurationErrorsException)
            {
                LogMessage($"Error writing app settings [{key}:{value}]", LogTypes.Error);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            LogMessage($"Starting...", LogTypes.Info);
            if (Directory.Exists(txtInput.Text))
            {
                foreach (var item in Directory.GetFiles(txtInput.Text, "*.pdf"))
                {
                    FileTypes fileType;
                    Enum.TryParse<FileTypes>(cmbOutputType.SelectedValue.ToString(), out fileType);

                    _delivery.RunFile(txtInput.Text, item, fileType);
                }
            }
            else
            {
                LogMessage($"{txtInput.Text} is an invalid path", LogTypes.Error);
            }

            LogMessage($"Finish!", LogTypes.Info);
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
