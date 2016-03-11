using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using CprBroker.Engine.Queues;
using mass_pnr_lookup.Parsers;

namespace mass_pnr_lookup.Models
{
    public class Batch
    {
        [Key]
        public int BatchId { get; set; }

        public BatchStatus Status { get; set; }

        public DateTime SubmittedTS { get; set; }
        public DateTime? CompletedTS { get; set; }

        public int Size { get; set; }
        public string FileName { get; set; }

        public byte[] SourceContents { get; set; }
        public byte[] GeneratedContents { get; set; }

        public Guid GenerationSemaphoreId { get; set; }
        public Guid NotificationSemaphoreId { get; set; }

        public User User { get; set; }

        public virtual ICollection<BatchLine> Lines { get; set; }

        public Parsers.IParser CreateParser()
        {
            return new Parsers.CsvParser(this.SourceContents);
        }

        public Semaphore GenerationSemaphore()
        {
            return Semaphore.GetById(GenerationSemaphoreId);
        }

        public Semaphore NotificationSemaphore()
        {
            return Semaphore.GetById(NotificationSemaphoreId);
        }

        public void GenerateOutput()
        {
            var b = new System.Text.StringBuilder((int)(Size * 1.2));
            var parser = new CsvParser.CsvEnumerator(SourceContents);
            b.Append(parser.FirstLine);
            b.AppendLine(";PNR;FEJL");

            foreach (var line in Lines.OrderBy(l => l.Row).ThenBy(l => l.BatchElementId))
            {
                b.Append(line.SourceContents);
                b.AppendFormat(";{0};{1}{2}", line.PNR, line.Error, Environment.NewLine);
            }
            GeneratedContents = System.Text.Encoding.UTF8.GetBytes(b.ToString());
        }

    }
}