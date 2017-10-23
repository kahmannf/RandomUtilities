using RandomUtilities.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilities.Serialization.CSV
{
    public delegate object HandleCostumTypeEventHandler(Type t, string value);

    public class CSVReader<T> : IDataReader<T>
        where T : new()
    {
        public const char SEPERATOR_CHAR = ';';
        public const char ESCAPE_QUOTES = '"';

        
        public CSVReader()
        {
            _headerRow = true;
        }

        public CSVReader(ColumnMapper<T> mapper)
        {
            _mapper = mapper;
        }

        private ColumnMapper<T> _mapper;
        public ColumnMapper<T> Mapper
        {
            get => _mapper;
            set => _mapper = value;
        }


        private bool _headerRow;
        public bool HeaderRow
        {
            get => _headerRow;
            set => _headerRow = value;
        }

        private FILO<string> _lineQueue;

        private FILO<T> _results;

        private FILO<Dictionary<int, string>> _lineColumns;

        private int _linesRead;

        private bool _lineReadComplete;

        private bool _headerRead;

        public event HandleCostumTypeEventHandler HandleUnknownType;

        public T[] Read(Stream s)
        {
            ResetValues();

            StartAsyncProcesses();

            ReadLines(s);

            WaitForAsync();

            return _results.Clear();
        }

        private void ResetValues()
        {
            _results = new FILO<T>();
            _lineQueue = new FILO<string>();
            _lineColumns = new FILO<Dictionary<int, string>>();
            _linesRead = 0;
            _lineReadComplete = false;
            _headerRead = false;
        }

        private void StartAsyncProcesses()
        {
            Task.Run(() => ReadSingleLineInternalLoop());
            Task.Run(() => ParseSingleLineInternal());
        }

        private void ReadLines(Stream s)
        {

            string line = string.Empty;

            int quoteCount = 0;

            while(s.ReadByte() is int value && value != -1)
            {
                char cvalue = (char)value;

                if (cvalue == ESCAPE_QUOTES)
                {
                    quoteCount++;
                }
                else if (cvalue == '\n' && quoteCount % 2 == 0)
                {

                    if (_headerRow && !_headerRead)//fill the mapper
                    {
                        if (_mapper == null)
                        {
                            _mapper = new ColumnMapper<T>();
                        }

                        Dictionary<int, string> headerColumns = ParseSingleLineInternal(line);

                        PropertyInfo[] properties = typeof(T).GetProperties();

                        foreach (int columnKey in headerColumns.Keys)
                        {
                            if (!_mapper.HasMapping(columnKey))
                            {
                                if (properties.FirstOrDefault(x => x.Name.ToLower() == headerColumns[columnKey].ToLower()) is PropertyInfo info)
                                {
                                    _mapper.Add(columnKey, info.Name);
                                }
                            }
                        }

                        _headerRead = true;
                    }
                    else
                    {
                        _linesRead++;
                        _lineQueue.Push(line);
                    }

                    line = string.Empty;

                    quoteCount = 0;

                }
                else
                {
                    line += cvalue;
                }
            }

            _lineReadComplete = true;
        }

        private void WaitForAsync()
        {
            while (!_lineReadComplete || _linesRead > _results.Count())
            {
                Thread.Sleep(10);
            }
        }

        private void ReadSingleLineInternalLoop()
        {
            while (!_lineReadComplete || _results.Count() < _linesRead)
            {
                if (_lineQueue.HasItems)
                {
                    string line = _lineQueue.Pull();
                    
                    _lineColumns.Push(ParseSingleLineInternal(line));
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private Dictionary<int, string> ParseSingleLineInternal(string line)
        {
            line = line.Trim('\n').Trim('\r').Trim('\n');

            Dictionary<int, string> columns = new Dictionary<int, string>();

            string currentColumn = string.Empty;

            int currentIndex = 0;

            bool escaped = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ESCAPE_QUOTES)
                {
                    escaped = !escaped;
                }
                if (line[i] == SEPERATOR_CHAR && !escaped)
                {
                    columns.Add(currentIndex, currentColumn);

                    currentIndex++;
                    currentColumn = string.Empty;
                }
                else
                {
                    currentColumn += line[i];
                }
            }
            if (!string.IsNullOrEmpty(currentColumn))
            {
                columns.Add(currentIndex, currentColumn);
            }

            return columns;
        }


        private void ParseSingleLineInternal()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            while (!_lineReadComplete || _results.Count() < _linesRead)
            {
                if (_lineColumns.HasItems)
                {
                    T result = new T();

                    Dictionary<int, string> columns = _lineColumns.Pull();

                    foreach (int mappedKey in _mapper.MappedColumns)
                    {
                        if (columns.ContainsKey(mappedKey) && properties.FirstOrDefault(x => x.Name == _mapper[mappedKey]) is PropertyInfo property)
                        {
                            SetProperty(property, result, columns[mappedKey]);
                        }
                    }

                    _results.Push(result);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void SetProperty(PropertyInfo property, T parent, string value)
        {
            Type type = property.PropertyType;

            if (type == typeof(string))
            {
                property.SetValue(parent, value);
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(value, out int i))
                {
                    property.SetValue(parent, i);
                }
            }
            else if (type == typeof(double))
            {
                if (double.TryParse(value, out double d))
                {
                    property.SetValue(parent, d);
                }
            }
            else
            {
                if (HandleUnknownType != null)
                {
                    object o = HandleUnknownType(type, value);
                    property.SetValue(parent, o);
                }
            }
        }



    }
}
