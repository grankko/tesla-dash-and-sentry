using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaCamMap.UwpClient.ClientEventArgs
{
    public class ProgressEventArgs : EventArgs
    {
        public int ItemsCompleted { get; private set; }
        public int ItemsFailed { get; private set; }
        public int ItemsTotal { get; private set; }

        public ProgressEventArgs(int itemsCompleted, int itemsFailed, int itemsTotal)
        {
            ItemsCompleted = itemsCompleted;
            ItemsTotal = itemsTotal;
            ItemsFailed = itemsFailed;
        }
    }
}
