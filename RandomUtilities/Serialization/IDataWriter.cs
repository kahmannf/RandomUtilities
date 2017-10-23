using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Serialization
{
    public interface IDataWriter<T>
    {
        void Write(Stream s, T item);
        void Write(Stream s, T[] items);
    }
}
