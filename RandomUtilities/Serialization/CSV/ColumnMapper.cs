using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Serialization.CSV
{
    public class ColumnMapper<T>
    {
        public ColumnMapper()
        {
            _mappers = new Dictionary<int, string>();
        }

        private Dictionary<int, string> _mappers;

        public void Add(int column, string propertyName)
        {
            _mappers.Add(column, propertyName);
        }

        public bool HasMapping(int column)
        {
            return _mappers.ContainsKey(column);
        }

        public string this[int column]
        {
            get
            {
                if (!HasMapping(column))
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return _mappers[column];
                }
            }
        }

        public IEnumerable<int> MappedColumns => _mappers.Keys;

    }
}
