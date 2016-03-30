using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CprBroker.Engine.Queues;
using mass_pnr_lookup.Queues;

namespace mass_pnr_lookup
{
    public class QueueRegistration
    {
        public static void Register()
        {

            var extrationQueue = Queue.GetQueues<ExtractionQueue>().SingleOrDefault();
            if (extrationQueue == null)
            {
                extrationQueue = Queue.AddQueue<ExtractionQueue>(3000, new Dictionary<string, string>(), 1, 100);
            }

            var searchQueues = Queue.GetQueues<SearchQueue>().ToList();
            while (searchQueues.Count < Properties.Settings.Default.NumberOfSearchQueues)
            {
                var searchQueue = Queue.AddQueue<SearchQueue>(3000, new Dictionary<string, string>(), 25, 1);
                searchQueues.Add(searchQueue);
            }

            var outputGenerationQueue = Queue.GetQueues<OutputGenerationQueue>().SingleOrDefault();
            if (outputGenerationQueue == null)
            {
                outputGenerationQueue = Queue.AddQueue<OutputGenerationQueue>(3000, new Dictionary<string, string>(), 1, 5);
            }

            var userNotificationQueue = Queue.GetQueues<UserNotificationQueue>().SingleOrDefault();
            if (userNotificationQueue == null)
            {
                userNotificationQueue = Queue.AddQueue<UserNotificationQueue>(3000, new Dictionary<string, string>(), 1, 5);
            }
        }
    }
}