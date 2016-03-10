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
                try
                {
                    using (var context = new BatchContext())
                    {
                        var batch = context.Batches.Find(item.BatchId);
                        var parser = batch.CreateParser();

                        var lines = parser.ToArray();
                        batch.Lines = lines;
                        context.SaveChanges();

                        var searchQueue = Queue.GetQueues<SearchQueue>().FirstOrDefault();
                        foreach (var line in lines)
                            searchQueue.Enqueue(line.ToQueueItem());

                        ret.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            return ret.ToArray();
        }
    }
}