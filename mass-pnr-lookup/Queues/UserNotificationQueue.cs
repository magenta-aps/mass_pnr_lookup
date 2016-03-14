using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Queues
{
    public class UserNotificationQueue : CprBroker.Engine.Queues.Queue<BatchQueueItem>
    {
        public override BatchQueueItem[] Process(BatchQueueItem[] items)
        {
            var ret = new List<BatchQueueItem>();
            foreach (var item in items)
            {
                // TODO: Notify user here
                ret.Add(item);
            }
            return ret.ToArray();
        }
    }
}