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
        public Guid SearchSemaphoreId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<BatchLine> Lines { get; set; }

        public Parsers.IParser CreateParser()
        {
            if (FileName.EndsWith(".csv"))
                return new Parsers.CsvParser(SourceContents);
            else if (FileName.EndsWith(".xlsx"))
                return new XlsxParser(SourceContents);
            else
                return null;
        }

        public Semaphore GenerationSemaphore()
        {
            return Semaphore.GetById(GenerationSemaphoreId);
        }

        public Semaphore NotificationSemaphore()
        {
            return Semaphore.GetById(NotificationSemaphoreId);
        }

        public Semaphore SearchSemaphore()
        {
            return Semaphore.GetById(SearchSemaphoreId);
        }

        public void GenerateOutput()
        {
            using (var parser = CreateParser())
            {
                var newColumnNames = new string[] { "PNR", "FEJL", "EJER_NAVN_MATCH", "EJER_ADR_MATCH" };
                foreach (var colName in newColumnNames)
                {
                    if (!parser.ContentsTable.Columns.Contains(colName))
                    {
                        parser.ContentsTable.Columns.Add(colName, typeof(string));
                    }
                }

                int index = 0;
                foreach (var line in Lines.OrderBy(l => l.Row).ThenBy(l => l.BatchElementId))
                {
                    var row = parser.ContentsTable.Rows[index++];
                    var values = new string[] { line.PNR, line.Error, line.MatchedName, line.MatchedAddress };
                    for (int i = 0; i < values.Length; i++)
                    {
                        row[newColumnNames[i]] = values[i];
                    }
                }
                GeneratedContents = parser.SerializeContents();
            }
        }

        public void EnqueueOutputGeneration()
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
            var searchSemaphore = Semaphore.Create();
            searchSemaphore.SignalAll();
            this.SearchSemaphoreId = searchSemaphore.Impl.SemaphoreId;

            var searchQueues = Queue.GetQueues<SearchQueue>();
            if (searchQueues.Length < 1)
                throw new Exception("Search queues not found");

            var random = new Random();
            var index = random.Next(0, searchQueues.Length);

            foreach (var line in this.Lines)
            {
                searchQueues[index].Enqueue(line.ToQueueItem(), searchSemaphore);
                index = (index + 1) % searchQueues.Length;
            }
        }

        public void ResetCounters()
        {
            this.NumLines = this.Lines.Count;
            this.FailedLines = 0;
            this.SucceededLines = 0;
            this.CompletedTS = null;
            this.Status = BatchStatus.Processing;
        }

        public void ResetResults()
        {
            foreach (var line in Lines)
            {
                line.ClearResults();
            }
        }

        public void EnqueueAllAfterExtraction(BatchContext context)
        {
            this.ResetCounters();
            this.ResetResults();
            context.SaveChanges();

            this.EnqueueOutputGeneration();
            this.EnqueueNotification();
            context.SaveChanges();

            this.EnqueueSearch();
            context.SaveChanges();
        }

    }
}