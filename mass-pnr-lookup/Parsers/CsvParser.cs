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

        public class CsvEnumerator : IEnumerator<BatchLine>
        {
            public byte[] Contents { get; private set; }

            StreamReader _StreamReader;
            MemoryStream _MemoryStream;

            string _CurrentLine;
            int _CurrentLineIndex;

            int nameColumnIndex = 1;
            int addressColumnIndex = 2;
            int postAddressColumnIndex = 3;

            public string FirstLine;

            public CsvEnumerator(byte[] contents)
            {
                if (contents == null)
                    contents = new byte[0];

                this.Contents = contents;
                this._MemoryStream = new MemoryStream(this.Contents);
                this._StreamReader = new StreamReader(this._MemoryStream, Commons.CsvEncoding);

                // Read the first line to get column indecies

                while (FirstLine == null && !this._StreamReader.EndOfStream)
                {
                    FirstLine = this._StreamReader.ReadLine();
                }
                if (!string.IsNullOrEmpty(FirstLine))
                {
                    var firstLineValues = FirstLine.Split(';');
                    nameColumnIndex = Array.IndexOf<string>(firstLineValues, "EJER_NAVN");
                    addressColumnIndex = Array.IndexOf<string>(firstLineValues, "EJER_ADR");
                    postAddressColumnIndex = Array.IndexOf<string>(firstLineValues, "EJER_POSTADR");
                }
                else
                {
                    throw new ArgumentException("Invalid contents");
                }
            }

            public BatchLine Current
            {
                get
                {
                    if (string.IsNullOrEmpty(_CurrentLine))
                        return null as BatchLine;

                    var values = _CurrentLine.Split(';');

                    return new BatchLine()
                    {
                        Name = values.Skip(nameColumnIndex).Take(1).FirstOrDefault(),
                        Address = string.Format("{0}, {1}",
                            values.Skip(addressColumnIndex).Take(1).FirstOrDefault(),
                            values.Skip(postAddressColumnIndex).Take(1).FirstOrDefault()
                            ),
                        Row = _CurrentLineIndex,
                        SourceContents = _CurrentLine
                    };
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
                        _CurrentLineIndex++;
                    }
                }
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }


}