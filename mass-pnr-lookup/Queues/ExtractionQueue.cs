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
                        // Delete existing lines if needed
                        var oldLines = context.BatchLines.Where(bl => bl.Batch_BatchId == batch.BatchId);
                        context.BatchLines.RemoveRange(oldLines);
                        context.SaveChanges();

                        context.Entry<Batch>(batch).Reload();

                        var lines = parser.ToArray();
                        if (batch.Lines == null)
                            batch.Lines = new List<BatchLine>();

                        Array.ForEach<BatchLine>(lines, bl => batch.Lines.Add(bl));

                        batch.EnqueueAllAfterExtraction(context);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);

                        batch.Status = BatchStatus.Error;
                        context.SaveChanges();
                    }
                    ret.Add(item);
                }
            }
            return ret.ToArray();
        }
    }
}