using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mass_pnr_lookup.Queues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class QueueTests
    {
        [TestMethod]
        public void RunQueues()
        {
            CprBroker.Engine.BrokerContext.Initialize(CprBroker.Utilities.Constants.BaseApplicationToken.ToString(), "Testing developer");

            CprBroker.Engine.Queues.Queue.GetQueues<ExtractionQueue>().Single().RunAll();
            foreach (var searchQueue in CprBroker.Engine.Queues.Queue.GetQueues<SearchQueue>())
                searchQueue.RunAll();
            CprBroker.Engine.Queues.Queue.GetQueues<OutputGenerationQueue>().Single().RunAll();
            CprBroker.Engine.Queues.Queue.GetQueues<UserNotificationQueue>().Single().RunAll();
        }
    }
}
