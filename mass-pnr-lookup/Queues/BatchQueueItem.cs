using CprBroker.Engine.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Queues
{
    public class BatchQueueItem : QueueItemBase
    {
        public override string SerializeToKey()
        {
            return string.Format("{0}", BatchId);
        }

        public override void DeserializeFromKey(string key)
        {
            BatchId = int.Parse(key);
        }

        public int BatchId { get; set; }
    }
}