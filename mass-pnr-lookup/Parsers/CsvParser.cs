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

        public class CsvEnumerator : BaseEnumerator, IEnumerator<BatchLine>
        {

            StreamReader _StreamReader;

            string _CurrentLine;
            public string FirstLine;

            public CsvEnumerator(byte[] contents)
                : base(contents)
            {

            }

            public override void CustomInit()
            {
                this._StreamReader = new StreamReader(this._MemoryStream, Commons.CsvEncoding);
            }

            public override string[] ReadColumnNames()
            {
                while (FirstLine == null && !this._StreamReader.EndOfStream)
                {
                    FirstLine = this._StreamReader.ReadLine();
                }
                if (!string.IsNullOrEmpty(FirstLine))
                {
                    var firstLineValues = FirstLine.Split(';');
                    return firstLineValues;
                }
                else
                {
                    throw new ArgumentException("Invalid contents");
                }
            }

            public override string[] ReadCurrentValues()
            {
                if (string.IsNullOrEmpty(_CurrentLine))
                    return null;

                return _CurrentLine.Split(';');
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public override void Dispose()
            {
                if (_StreamReader != null)
                    _StreamReader.Dispose();

                base.Dispose();
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