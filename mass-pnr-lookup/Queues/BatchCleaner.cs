using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Engine.Tasks;
using mass_pnr_lookup.Models;
using CprBroker.Engine.Local;

namespace mass_pnr_lookup.Queues
{
    public class BatchCleaner : PeriodicTaskExecuter
    {
        protected override void PerformTimerAction()
        {

            int daysToKeepFiles = Properties.Settings.Default.DaysToKeepBatchFiles;

            DateTime deleteOlderThan = DateTime.Now.AddDays(-daysToKeepFiles);
            using (BatchContext context = new BatchContext())
            {
                IQueryable<Batch> batchesToDelete = context.Batches.Where(b => b.CompletedTS < deleteOlderThan);
                
                foreach(Batch batch in batchesToDelete)
                {
                    try
                    { 
                        context.Batches.Remove(batch);
                        batch.SignalAllSemaphores();
                        Admin.LogSuccess("Mass PNR Lookup: Removed batch " + batch.BatchId);
                    } catch(Exception ex)
                    {
                        Admin.LogException(ex, String.Format("Mass PNR Lookup Exception: {0}", ex.ToString()));
                    }
                }
                context.SaveChanges();


            }
        }
    }
}
