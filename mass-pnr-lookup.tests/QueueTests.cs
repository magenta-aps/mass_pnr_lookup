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
    class QueueTests
    {
        [TestMethod]
        public void RunQueues()
        {
            CprBroker.Engine.BrokerContext.Initialize(CprBroker.Utilities.Constants.BaseApplicationToken.ToString(), "Testing developer");

            CprBroker.Engine.Queues.Queue.GetQueues<ExtractionQueue>().Single().RunAll();
            CprBroker.Engine.Queues.Queue.GetQueues<SearchQueue>().Single().RunAll();
            CprBroker.Engine.Queues.Queue.GetQueues<OutputGenerationQueue>().Single().RunAll();
            CprBroker.Engine.Queues.Queue.GetQueues<UserNotificationQueue>().Single().RunAll();
        }
    }
}
