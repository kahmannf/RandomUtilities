using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Queue
{
    class QueueAccessResult<T>
    {
        public QueueAccesTypes Type { get; set; }
        public T Item { get; set; }
        public T[] Items { get; set; }
        public bool WasSuccessful { get; set; }
        public string FailReason { get; set; }
        public Exception Exception { get; set; }
    }
}
