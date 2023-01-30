using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCodeSplitter.lib.entities
{
    public enum ItemStatus
    {
        Started = 0,
        Splitting = 1,
        SearchingBarCode = 2,
        SearchingText = 3,
        ProcessingFileType = 4,
        CreatingSummary = 5,
        Done = 6
    }

    public class ItemProcess
    {
        public string FileName { get; set; }
        public ItemStatus Status { get; set; }
        public int PageCount { get; set; }
        public int Thermal { get; set; }    
        public int Paper {  get; set; }
        public DateTime StatedAt { get; set; }  
        public DateTime LastUpdate {  get; set; }
        public DateTime? FinishAt { get; set; } = null;
    }
}
