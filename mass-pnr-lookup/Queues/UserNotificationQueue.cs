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
            throw new NotImplementedException();
        }
    }
}