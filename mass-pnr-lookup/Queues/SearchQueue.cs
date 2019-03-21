using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.PartInterface;
using CprBroker.Engine.Queues;
using mass_pnr_lookup.Models;
using CprBroker.Engine;
using CprBroker.Schemas.Part;
using CprBroker.Schemas;

namespace mass_pnr_lookup.Queues
{
    public class SearchQueue : CprBroker.Engine.Queues.Queue<LineQueueItem>
    {
        private bool search(PartManager partManager, SoegInputType1 soegObject, BatchLine batchLine, SourceUsageOrder SourceUsageOrder)
        {
            var searchResult = partManager.SearchList(
                            BrokerContext.Current.UserToken,
                            BrokerContext.Current.ApplicationToken,
                            soegObject,
                            SourceUsageOrder);

            if (StandardReturType.IsSucceeded(searchResult.StandardRetur))
            {
                return batchLine.FillFrom(searchResult);
                
            }
            else
            {
                batchLine.Error = string.Format("{0}-{1}", searchResult.StandardRetur.StatusKode, searchResult.StandardRetur.FejlbeskedTekst);
                return false;
            }

        }

        public override LineQueueItem[] Process(LineQueueItem[] items)
        {
            var ret = new List<LineQueueItem>();

            using (var context = new BatchContext())
            {
                foreach (var item in items)
                {
                    bool itemSucceeded = false;

                    var batchLine = context.BatchLines.Find(item.BatchLineId);

                    if (batchLine == null)
                    {
                        if (item.Impl.AttemptCount >= this.Impl.MaxRetry - 1)
                        {
                            // Max attempts reached - signal and remove anyway
                            ret.Add(item);
                        }
                        continue;
                    }

                    var partManager = new PartManager();
                    var soegObject = batchLine.ToSoegObject();


                    if (soegObject != null)
                    {
                        // Try to search locally first
                        // Since Mass PNR Lookup does some name-matching itself first, we need the local search.
                        // There could be a local person found by CPR Broker, that Mass PNR Lookup does not match, therefore using "SourceUsageOrder.LocalThenExternal" is not enough
                        if (search(partManager, soegObject, batchLine, SourceUsageOrder.LocalOnly))
                        {
                            itemSucceeded = true;
                        } else
                        {
                            // If no local person was found, search Externally
                            if (search(partManager, soegObject, batchLine, SourceUsageOrder.ExternalOnly))
                            {
                                itemSucceeded = true;
                            }
                        }

                    }
                    else
                    {
                        batchLine.Error = "Invalid address";
                    }

                    lock ("BatchCounts")
                    {
                        context.Entry<Batch>(batchLine.Batch).Reload(); // Reload to avoid overwriting the counts

                        if (itemSucceeded)
                            batchLine.Batch.SucceededLines++;

                        // Save the result at this point
                        context.SaveChanges();

                        // Queue management
                        if (itemSucceeded)
                        {
                            ret.Add(item);
                            // Decrement the wait count on the semaphore
                            batchLine.Batch.GenerationSemaphore().Signal();
                        }
                        else if (item.Impl.AttemptCount >= this.Impl.MaxRetry - 1)
                        {
                            // Max attempts reached - signal and remove anyway
                            ret.Add(item);
                            batchLine.Batch.FailedLines++;
                            context.SaveChanges();

                            batchLine.Batch.GenerationSemaphore().Signal();
                        }
                    }
                }
            }
            return ret.ToArray();
        }
    }
}