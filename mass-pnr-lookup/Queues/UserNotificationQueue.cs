using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mass_pnr_lookup.Models;
using System.DirectoryServices.AccountManagement;
using System.Net.Mail;
using CprBroker.Engine.Local;

namespace mass_pnr_lookup.Queues
{
    public class UserNotificationQueue : CprBroker.Engine.Queues.Queue<BatchQueueItem>
    {
        public override BatchQueueItem[] Process(BatchQueueItem[] items)
        {
            var ret = new List<BatchQueueItem>();
            foreach (var item in items)
            {
                try
                {
                    using (var context = new BatchContext())
                    {
                        var batch = context.Batches.Find(item.BatchId);

                        // If the batch is deleted
                        if(batch == null)
                        {
                            if (item.Impl.AttemptCount >= this.Impl.MaxRetry - 1)
                            {
                                // Max retry reached, clean up queueItem
                                ret.Add(item);

                            }
                            continue;
                            
                        }

                        // If this item is the latest assigned notification queue item
                        if (batch.NotificationSemaphore().Impl.SemaphoreId == item.Impl.SemaphoreId.Value)
                        {
                            var userPrincipal = batch?.User?.GetUserPrincipal(ContextType.Domain);
                            var email = userPrincipal?.EmailAddress;

                            if (!string.IsNullOrEmpty(email))
                            {
                                Admin.LogFormattedSuccess(
                                        "Sending email for batch <{0}>, user <{1}>, principal <{2}>, email <{3}>",
                                        batch.BatchId,
                                        batch.User?.Name,
                                        userPrincipal?.Name,
                                        email
                                        );

                                // Create client and message
                                var smtpClient = new SmtpClient();

                                MailMessage msg = new MailMessage()
                                {
                                    //From = new MailAddress((smtpClient.Credentials as System.Net.NetworkCredential).UserName),
                                    Subject = "Batch completed",
                                    Body = string.Format("Your batch '{0}' has been completed at {1}", batch.FileName, batch.CompletedTS),
                                    From = new MailAddress(email)
                                };
                                msg.To.Add(new MailAddress(email, userPrincipal.DisplayName));

                                // Send the message
                                smtpClient.Send(msg);

                                // Update the status
                                batch.Status = BatchStatus.Notified;
                                context.SaveChanges();
                                ret.Add(item);
                            }
                            else
                            {
                                Admin.LogFormattedError(
                                    "Could not find email for batch <{0}>, user <{1}>, principal <{2}>, email <{3}>",
                                    batch?.BatchId,
                                    batch?.User?.Name,
                                    userPrincipal?.Name,
                                    email
                                    );
                                if (item.Impl.AttemptCount >= this.Impl.MaxRetry - 1)
                                {
                                    // Max retry reached, clean up queueItem
                                    ret.Add(item);

                                }
                            }
                        }
                        else
                        {
                            Admin.LogFormattedError(
                                "Semaphore mismatch, skipping user notificatiuons. Batch <{0}> queue item <{1}>",
                                    batch.BatchId,
                                    item.Impl.QueueItemId);
                            if (item.Impl.AttemptCount >= this.Impl.MaxRetry - 1)
                            {
                                // Max retry reached, clean up queueItem
                                ret.Add(item);

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Admin.LogException(ex, string.Format("queue item <{0}>", item.Impl.QueueItemId));
                    if (item.Impl.AttemptCount >= this.Impl.MaxRetry - 1)
                    {
                        // Max retry reached, clean up queueItem
                        ret.Add(item);

                    }
                }
            }
            return ret.ToArray();
        }
    }
}