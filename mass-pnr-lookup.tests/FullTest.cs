using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mass_pnr_lookup.Controllers;
using mass_pnr_lookup.Models;
using mass_pnr_lookup.Queues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using CprBroker.Engine.Queues;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class FullTest
    {
        [TestMethod]
        public void Run()
        {
            string fileName = Guid.NewGuid().ToString();
            CprBroker.Engine.BrokerContext.Initialize(CprBroker.Utilities.Constants.BaseApplicationToken.ToString(), "");
            using (var myContext = new BatchContext())
            {
                Func<Batch> batch = () =>
                {
                    return myContext.Batches.Where(b => b.FileName == fileName).SingleOrDefault();
                };

                var controller = new FilesController();
                var bytes = Encoding.UTF8.GetBytes(Properties.Resources.Test_Opslag);

                controller.EnqueueFile(new MemoryStream(bytes), fileName, bytes.Length, "dummyUser");

                Assert.IsNotNull(batch());

                // Extract
                var extractQueue = Queue.GetQueues<ExtractionQueue>().Single();
                var extractQueueItem = extractQueue.GetNext(1000).Where(qi0 => qi0.BatchId == batch().BatchId).SingleOrDefault();
                Assert.IsNotNull(extractQueueItem);

                var extractResult = extractQueue.Process(new BatchQueueItem[] { extractQueueItem });
                Assert.AreEqual(1, extractResult.Length);
                extractQueue.Remove(extractResult);

                // Search
                var searchQueue = Queue.GetQueues<SearchQueue>().Single();
                var searchQueueItems = searchQueue.GetNext(1000).Where(qi => batch().Lines.Select(l => l.BatchElementId).Contains(qi.BatchLineId)).ToArray();
                Assert.AreEqual(batch().Lines.Count, searchQueueItems.Length);

                var searchResult = searchQueue.Process(searchQueueItems);
                Assert.AreEqual(batch().Lines.Count, searchResult.Length);
                searchQueue.Remove(searchResult);

                // Output generation
                var outputGenerationQueue = Queue.GetQueues<OutputGenerationQueue>().Single();
                var outputGenerationQueueItem = outputGenerationQueue.GetNext(1000).Where(qi => qi.BatchId == batch().BatchId).SingleOrDefault();
                Assert.IsNotNull(outputGenerationQueueItem);
                var outputGenerationResult = outputGenerationQueue.Process(new BatchQueueItem[] { outputGenerationQueueItem });
                Assert.Equals(1, outputGenerationResult.Length);

                // Notification
                var notificationQueue = Queue.GetQueues<UserNotificationQueue>().Single();
                var notificationQueueItem = notificationQueue.GetNext(1000).Where(qi => qi.BatchId == batch().BatchId).SingleOrDefault();
                Assert.IsNotNull(notificationQueueItem);
                var notificationResult = notificationQueue.Process(new BatchQueueItem[] { notificationQueueItem });
                Assert.Equals(1, notificationResult.Length);

            }
        }
    }
}