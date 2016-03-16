using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mass_pnr_lookup.Models;

namespace mass_pnr_lookup.Queues
{
    public class UserNotificationQueue : CprBroker.Engine.Queues.Queue<BatchQueueItem>
    {
        public override BatchQueueItem[] Process(BatchQueueItem[] items)
        {
            var ret = new List<BatchQueueItem>();
            foreach (var item in items)
            {
                using (var context = new BatchContext())
                {
                    var batch = context.Batches.Find(item.BatchId);

                    // TODO: Notify user here
                    //.....

                    batch.Status = BatchStatus.Notified;
                    context.SaveChanges();
                }
                ret.Add(item);
            }
            return ret.ToArray();
        }
    }
}