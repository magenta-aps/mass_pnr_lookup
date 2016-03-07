using CprBroker.Engine.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Queues
{
    public class LineQueueItem : QueueItemBase
    {
        public override void DeserializeFromKey(string key)
        {
            throw new NotImplementedException();
        }

        public override string SerializeToKey()
        {
            throw new NotImplementedException();
        }
    }
}