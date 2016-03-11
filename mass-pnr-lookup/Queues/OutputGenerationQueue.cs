﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Engine.Queues;
using mass_pnr_lookup.Models;

namespace mass_pnr_lookup.Queues
{
    public class OutputGenerationQueue : CprBroker.Engine.Queues.Queue<BatchQueueItem>
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
                        // TODO: set GeneratedContents here
                        // batch.GeneratedContents = ...

                        context.SaveChanges();

                        // Signal the notification
                        batch.NotificationSemaphore().Signal();
                    }
                    ret.Add(item);
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