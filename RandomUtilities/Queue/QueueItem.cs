using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Queue
{
    class QueueItem<T>
    {
        public QueueItem(T item)
        {
            this.Content = item;
        }

        public T Content { get; set; }
        public QueueItem<T> RelatedItem { get; set; }
    }
}
