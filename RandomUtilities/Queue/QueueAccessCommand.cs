using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Queue
{
    enum QueueAccesTypes { Push, Pull, Peek, Clear }
    class QueueAccessCommand<T>
    {
        private QueueAccesTypes clear;

        public QueueAccessCommand(QueueAccesTypes type)
        {
            this.Type = type;
        }

        public QueueAccessCommand(T item)
        {
            this.Item = item;
            this.Type = QueueAccesTypes.Push;
        }

        public QueueAccesTypes Type { get; set; }
        public T Item { get; set; }
    }
}
