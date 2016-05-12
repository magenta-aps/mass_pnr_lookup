using mass_pnr_lookup.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Parsers
{
    public abstract class BaseEnumerator : IDisposable
    {
        public byte[] Contents { get; private set; }

        protected MemoryStream _MemoryStream;

        protected int nameColumnIndex = 1;
        protected int addressColumnIndex = 2;
        protected int postAddressColumnIndex = 3;


        protected int _CurrentLineIndex;

        public BaseEnumerator(byte[] contents)
        {
            if (contents == null)
                contents = new byte[0];

            this.Contents = contents;
            this._MemoryStream = new MemoryStream(this.Contents);

            // Perform any custom initialization in the sub class
            CustomInit();

            // Read the first line/ row to get column indecies
            var firstLineValues = ReadColumnNames();

            nameColumnIndex = Array.IndexOf<string>(firstLineValues, "EJER_NAVN");
            addressColumnIndex = Array.IndexOf<string>(firstLineValues, "EJER_ADR");
            postAddressColumnIndex = Array.IndexOf<string>(firstLineValues, "EJER_POSTADR");
        }
        public virtual void CustomInit()
        { }

        public abstract string[] ReadColumnNames();
        public abstract string[] ReadCurrentValues();

        public virtual void Dispose()
        {
            if (_MemoryStream != null)
                ((IDisposable)_MemoryStream).Dispose();
        }

        public BatchLine Current
        {
            get
            {
                var values = ReadCurrentValues();
                if (values == null)
                    return null;

                return new BatchLine()
                {
                    Name = values.Skip(nameColumnIndex).Take(1).FirstOrDefault(),
                    Address = string.Format("{0}, {1}",
                        values.Skip(addressColumnIndex).Take(1).FirstOrDefault(),
                        values.Skip(postAddressColumnIndex).Take(1).FirstOrDefault()
                        ),
                    Row = _CurrentLineIndex,
                    // TODO: This is unnecessary for Xlsx
                    SourceContents = string.Join(";", values)
                };
            }
        }

    }
}