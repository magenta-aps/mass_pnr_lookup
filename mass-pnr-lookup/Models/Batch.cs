using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using CprBroker.Engine.Queues;

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

        public ICollection<BatchLine> Lines { get; set; }

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

    }
}