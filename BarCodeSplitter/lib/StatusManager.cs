using BarCodeSplitter.lib.entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCodeSplitter.lib
{
    public class StatusManager
    {
        private static StatusManager _instance;
        private readonly ConcurrentDictionary<string, ItemProcess> _data = new ConcurrentDictionary<string, ItemProcess>();

        public event EventHandler<ItemProcess> UpdateItem;

        public static StatusManager GetInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StatusManager();
                }
                return _instance;
            }
        }

        private ItemProcess GetItem(string fileName)
        {
            ItemProcess ret = null;
            var key = GetFileName(fileName);

            if (!_data.TryGetValue(key, out ret))
            {
                ret = new ItemProcess() { FileName = key, StatedAt = DateTime.Now, Status = ItemStatus.Started };
                _data.TryAdd(key, ret);    
            }

            return ret;
        }

        public void Clear()
        {
            _data.Clear();  
        }

        public Dictionary<string, ItemProcess> GetData()
        {
            return _data.ToDictionary(k => k.Key, v => v.Value);
        }

        private string GetFileName(string fileName)
        {
            return Path.GetFileName(fileName);
        }

        public void UpdateStatus(string fileName, ItemStatus newStatus)
        {
            var item = GetItem(fileName);
            item.Status = newStatus;
            item.LastUpdate = DateTime.Now;

            if (newStatus != ItemStatus.Done)
            {
                item.FinishAt = DateTime.Now;
            }

            Update(fileName, item);
        }

        public void UpdatePages(string fileName, int pages)
        {
            var item = GetItem(fileName);
            item.PageCount = pages;

            Update(fileName, item);
        }

        public void UpdateThermal(string fileName, int thermal)
        {
            var item = GetItem(fileName);            
            item.Thermal = thermal;

            Update(fileName, item);
        }

        public void UpdatePaper(string fileName, int paper)
        {
            var item = GetItem(fileName);
            item.Paper = paper;

            Update(fileName, item);
        }

        private void Update(string fileName, ItemProcess item)
        {
            var key = GetFileName(fileName);
            item.LastUpdate = DateTime.Now;
            _data[key] = item;
            UpdateItem?.Invoke(this, item);
        }
    }
}
