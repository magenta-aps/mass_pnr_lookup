using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using CprBroker.Engine.Queues;
using mass_pnr_lookup.Parsers;
using mass_pnr_lookup.Queues;

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

        public int NumLines { get; set; }
        public int SucceededLines { get; set; } = 0;
        public int FailedLines { get; set; } = 0;

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
                b.AppendFormat(";\"{0}\";{1}{2}", line.PNR, line.Error, Environment.NewLine);
            }
            GeneratedContents = Commons.CsvEncoding.GetBytes(b.ToString());
        }


        public void EnqueueExtraction()
        {
            // Output generation queue
            var genSemaphore = Semaphore.Create(this.Lines.Count);
            this.GenerationSemaphoreId = genSemaphore.Impl.SemaphoreId;

            Queue.GetQueues<OutputGenerationQueue>().Single().Enqueue(
                new BatchQueueItem() { BatchId = this.BatchId },
                genSemaphore
            );
        }

        public void EnqueueNotification()
        {
            // user notification queue
            var notifSemaphore = Semaphore.Create();
            this.NotificationSemaphoreId = notifSemaphore.Impl.SemaphoreId;

            Queue.GetQueues<UserNotificationQueue>().Single().Enqueue(
                new BatchQueueItem() { BatchId = this.BatchId },
                notifSemaphore
            );
        }

        public void EnqueueSearch()
        {
            // Search queue
            var searchQueue = Queue.GetQueues<SearchQueue>().FirstOrDefault();
            foreach (var line in this.Lines)
                searchQueue.Enqueue(line.ToQueueItem());
        }

        public void ResetCounters()
        {
            this.NumLines = this.Lines.Count;
            this.FailedLines = 0;
            this.SucceededLines = 0;
            this.CompletedTS = null;
            this.Status = BatchStatus.Processing;
        }

        public void EnqueueAllAfterExtraction(BatchContext context)
        {
            this.ResetCounters();
            context.SaveChanges();

            this.EnqueueExtraction();
            this.EnqueueNotification();
            context.SaveChanges();

            this.EnqueueSearch();
        }

    }
}