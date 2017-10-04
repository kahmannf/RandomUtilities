using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Queue
{
    public interface IQueue<T>
    {
        bool IsThreadsafe { get; }

        bool HasItems { get; }

        T Pull();
        T Peek();
        T[] Clear();
        void Push(T item);

        int Count();
        int Count(Predicate<T> p);

    }
}
