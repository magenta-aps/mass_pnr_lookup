using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mass_pnr_lookup.Models;
using mass_pnr_lookup.Queues;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class ExtractionQueueTests
    {

        public void Process_LargeBatch_OK(int batchSize)
        {
            var lines = Properties.Resources.Test_Opslag.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Take(2).ToList();
            StringBuilder b = new StringBuilder((lines[1].Length + 2) * batchSize);

            b.AppendLine(lines[0]);
            for (int i = 0; i < batchSize; i++)
                b.AppendLine(lines[1]);
            var bytes = Commons.CsvEncoding.GetBytes(b.ToString());
            var batch = new Batch() { FileName = "", Size = bytes.Length, SourceContents = bytes, SubmittedTS = DateTime.Now };
            using (var context = new BatchContext())
            {
                context.Batches.Add(batch);
                context.SaveChanges();
            }

            var queueItem = new Queues.BatchQueueItem() { BatchId = batch.BatchId };
            var queue = new ExtractionQueue();
            queue.Process(new BatchQueueItem[] { queueItem });
        }

        [TestMethod]
        public void Process_LargeBatch_100_OK()
        {
            Process_LargeBatch_OK(100);
        }

        [TestMethod]
        public void Process_LargeBatch_1000_OK()
        {
            Process_LargeBatch_OK(1000);
        }

        [TestMethod]
        public void Process_LargeBatch_5000_OK()
        {
            Process_LargeBatch_OK(5000);
        }

        [TestMethod]
        public void Process_LargeBatch_10000_OK()
        {
            Process_LargeBatch_OK(10000);
        }

        [TestMethod]
        public void Process_LargeBatch_20000_OK()
        {
            Process_LargeBatch_OK(20000);
        }
    }
}
