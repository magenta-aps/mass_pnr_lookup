using System;
using System.Collections.Generic;
using System.Linq;
using mass_pnr_lookup.Models;
using System.IO;
using System.Collections;

namespace mass_pnr_lookup.Parsers
{
    public class CsvParser : IParser
    {
        public byte[] Contents { get; private set; }

        public CsvParser(byte[] contents)
        {
            this.Contents = contents;
        }

        public IEnumerator<BatchLine> GetEnumerator()
        {
            return new CsvEnumerator(Contents);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class CsvEnumerator : IEnumerator<BatchLine>
        {
            public byte[] Contents { get; private set; }

            StreamReader _StreamReader;
            MemoryStream _MemoryStream;
            string _CurrentLine;

            public CsvEnumerator(byte[] contents)
            {
                if (contents == null)
                    contents = new byte[0];

                this.Contents = contents;
                this._MemoryStream = new MemoryStream(this.Contents);
                this._StreamReader = new StreamReader(this._MemoryStream);
            }

            public BatchLine Current
            {
                get
                {
                    if (string.IsNullOrEmpty(_CurrentLine))
                        return null as BatchLine;

                    var values = _CurrentLine.Split(';');

                    return new BatchLine(
                        name: values.Skip(0).Take(1).FirstOrDefault(),
                        address: values.Skip(1).Take(1).FirstOrDefault()
                        );
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                if (_StreamReader != null)
                    _StreamReader.Dispose();

                if (_MemoryStream != null)
                    _MemoryStream.Dispose();
            }

            public bool MoveNext()
            {
                _CurrentLine = null;

                while (string.IsNullOrEmpty(_CurrentLine))
                {
                    if (_StreamReader.EndOfStream)
                    {
                        return false;
                    }
                    else
                    {
                        _CurrentLine = _StreamReader.ReadLine();
                    }
                }
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }


}