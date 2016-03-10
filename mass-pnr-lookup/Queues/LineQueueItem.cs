using CprBroker.Engine.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Schemas.Part;

namespace mass_pnr_lookup.Queues
{
    public class LineQueueItem : QueueItemBase
    {
        public int BatchLineId { get; set; }

        public override void DeserializeFromKey(string key)
        {
            BatchLineId = int.Parse(key);
        }

        public override string SerializeToKey()
        {
            return string.Format("{0}", BatchLineId);
        }
    }
}