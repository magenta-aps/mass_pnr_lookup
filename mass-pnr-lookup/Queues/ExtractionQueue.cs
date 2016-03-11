using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Engine.Queues;
using mass_pnr_lookup.Models;

namespace mass_pnr_lookup.Queues
{
    public class ExtractionQueue : CprBroker.Engine.Queues.Queue<BatchQueueItem>
    {
        public override BatchQueueItem[] Process(BatchQueueItem[] items)
        {
            var ret = new List<BatchQueueItem>();
            foreach (var item in items)
            {
                using (var context = new BatchContext())
                {
                    var batch = context.Batches.Find(item.BatchId);
                    var parser = batch.CreateParser();

                    try
                    {
                        var lines = parser.ToArray();
                        batch.Lines = lines;

                        // Output generation queue
                        var genSemaphore = Semaphore.Create(lines.Length);
                        batch.GenerationSemaphoreId = genSemaphore.Impl.SemaphoreId;

                        GetQueues<OutputGenerationQueue>().Single().Enqueue(
                            new BatchQueueItem() { BatchId = batch.BatchId },
                            genSemaphore
                        );

                        // user notification queue
                        var notifSemaphore = Semaphore.Create();
                        batch.NotificationSemaphoreId = notifSemaphore.Impl.SemaphoreId;

                        GetQueues<UserNotificationQueue>().Single().Enqueue(
                            new BatchQueueItem() { BatchId = batch.BatchId },
                            notifSemaphore
                        );

                        context.SaveChanges();

                        // Search queue
                        var searchQueue = GetQueues<SearchQueue>().FirstOrDefault();
                        foreach (var line in lines)
                            searchQueue.Enqueue(line.ToQueueItem());

                        batch.Status = BatchStatus.Processing;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);

                        batch.Status = BatchStatus.Error;
                    }
                    context.SaveChanges();
                    ret.Add(item);
                }
            }
            return ret.ToArray();
        }
    }
}