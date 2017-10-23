using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Serialization
{
    public interface IDataReader<T>
        where T : new()
    {
        T[] Read(Stream s);
    }
}
