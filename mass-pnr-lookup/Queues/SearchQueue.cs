using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.PartInterface;
using CprBroker.Engine.Queues;
using mass_pnr_lookup.Models;
using CprBroker.Engine;
using CprBroker.Schemas.Part;

namespace mass_pnr_lookup.Queues
{
    public class SearchQueue : CprBroker.Engine.Queues.Queue<LineQueueItem>
    {
        public override LineQueueItem[] Process(LineQueueItem[] items)
        {
            var ret = new List<LineQueueItem>();

            using (var context = new BatchContext())
            {
                foreach (var item in items)
                {
                    bool itemSucceeded = false;

                    var batchLine = context.BatcheLines.Find(item.BatchLineId);

                    var partManager = new PartManager();
                    var soegObject = batchLine.ToSoegObject();

                    if (soegObject != null)
                    {
                        var searchResult = partManager.SearchList(
                            BrokerContext.Current.UserToken,
                            BrokerContext.Current.ApplicationToken,
                            soegObject,
                            CprBroker.Schemas.SourceUsageOrder.ExternalOnly);

                        if (StandardReturType.IsSucceeded(searchResult.StandardRetur))
                        {
                            // If multiple matches are found, choose the one with the shortest full name
                            var bestMatch = searchResult.LaesResultat
                                .OrderBy(le =>
                                    string.Join(" ",
                                    (le.Item as RegistreringType1).AttributListe.Egenskab.First().NavnStruktur.PersonNameStructure.ToArray())
                                    .Length)
                                .First()
                                .Item as RegistreringType1;

                            batchLine.PNR = bestMatch.AttributListe.GetPnr();
                            itemSucceeded = true;
                        }
                        else
                        {
                            batchLine.Error = string.Format("{0}-{1}", searchResult.StandardRetur.StatusKode, searchResult.StandardRetur.FejlbeskedTekst);
                        }
                    }
                    else
                    {
                        batchLine.Error = "Invalid address";
                    }
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
                        // Max attempts reached - signal anyway
                        batchLine.Batch.GenerationSemaphore().Signal();
                    }
                }
            }
            return ret.ToArray();
        }
    }
}