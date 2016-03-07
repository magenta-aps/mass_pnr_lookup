using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mass_pnr_lookup.Queues
{
    public class SearchQueue : CprBroker.Engine.Queues.Queue<LineQueueItem>
    {
        public override LineQueueItem[] Process(LineQueueItem[] items)
        {
            throw new NotImplementedException();
        }
    }
}