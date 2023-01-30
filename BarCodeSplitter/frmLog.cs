using BarCodeSplitter.lib;
using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarCodeSplitter
{
    public partial class frmLog : Form
    {
        public bool _enable = true;
        public frmLog()
        {
            InitializeComponent();

            StatusManager.GetInstance.UpdateItem += OnUpdateItem;
            dgvFiles.DataSource = _rows;

            Task.Factory.StartNew(() => UpdateGrid());
        }

        ~frmLog()
        {
            _enable = false;
        }


        private ConcurrentQueue<ItemProcess> _events = new ConcurrentQueue<ItemProcess>();
        private ConcurrentDictionary<string, ItemProcess> _inProcessItems = new ConcurrentDictionary<string, ItemProcess>(StatusManager.GetInstance.GetData());
        private List<ItemProcess> _rows = new List<ItemProcess>();
        private DateTime _lastUpdate = DateTime.MinValue;

        private void OnUpdateItem(object sender, ItemProcess item)
        {
            _events.Enqueue(item);
        }

        private void UpdateGrid()
        {
            ItemProcess item;
            while (_enable)
            {
                if (_events.TryDequeue(out item))
                {
                    if (dgvFiles.InvokeRequired)
                    {
                        dgvFiles.Invoke((MethodInvoker)delegate
                        {
                            UpdateRows(item);
                        });
                    }
                    else
                    {
                        UpdateRows(item);
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        private void UpdateRows(ItemProcess item)
        {
            _inProcessItems.AddOrUpdate(item.FileName, item, (k, v) => item);
            if (DateTime.Now >= _lastUpdate.AddSeconds(1))
            {
                _rows = _inProcessItems.Select(p => p.Value).OrderByDescending(p => p.LastUpdate).ToList();
                dgvFiles.DataSource = _rows;
                dgvFiles.Refresh();
                _lastUpdate = DateTime.Now;
            }
        }

    }
}
