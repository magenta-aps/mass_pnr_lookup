using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using mass_pnr_lookup.Models;
using System.DirectoryServices.AccountManagement;
using System.Net.Mail;

namespace mass_pnr_lookup.Queues
{
    public class UserNotificationQueue : CprBroker.Engine.Queues.Queue<BatchQueueItem>
    {
        public override BatchQueueItem[] Process(BatchQueueItem[] items)
        {
            var ret = new List<BatchQueueItem>();
            foreach (var item in items)
            {
                using (var context = new BatchContext())
                {
                    var batch = context.Batches.Find(item.BatchId);
                    // If this item is the latest assigned notification queue item
                    if (batch.NotificationSemaphore().Impl.SemaphoreId == item.Impl.SemaphoreId.Value)
                    {
                        var userPrincipal = batch?.User?.GetUserPrincipal(ContextType.Domain);
                        var email = userPrincipal?.EmailAddress;

                        if (!string.IsNullOrEmpty(email))
                        {
                            // Create client and message
                            var smtpClient = new SmtpClient();

                            MailMessage msg = new MailMessage()
                            {
                                //From = new MailAddress((smtpClient.Credentials as System.Net.NetworkCredential).UserName),
                                Subject = "Batch completed",
                                Body = string.Format("Your batch '{0}' has been completed at {1}", batch.FileName, batch.CompletedTS),
                                From = new MailAddress(email, userPrincipal.DisplayName)
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
                            try
                            {
                                CprBroker.Engine.Local.Admin.LogFormattedError(
                                    "Could not find email for batch <{0}>, user <{1}>, principal <{2}>, email <{3}>",
                                    batch.BatchId,
                                    batch.User?.Name,
                                    userPrincipal?.Name,
                                    email
                                    );
                            }
                            catch (Exception ex)
                            {
                                CprBroker.Engine.Local.Admin.LogException(ex);
                            }
                        }
                    }
                }
            }
            return ret.ToArray();
        }
    }
}