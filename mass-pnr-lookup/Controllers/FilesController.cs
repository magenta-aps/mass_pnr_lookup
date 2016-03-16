using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mass_pnr_lookup.Models;

namespace mass_pnr_lookup.Controllers
{
    public class FilesController : Controller
    {

        // GET: Files
        public ActionResult Index(bool allUsers = false)
        {
            return View(allUsers);
        }

        public ActionResult List(bool allUsers = false)
        {
            IEnumerable<Batch> ret;
            using (var context = new Models.BatchContext())
            {
                if (allUsers)
                {
                    ret = context.Batches
                        .OrderByDescending(b => b.SubmittedTS).ToArray();
                }
                else
                {
                    var user = GetUser(context, User.Identity.Name);

                    ret = user.Batches
                        .OrderByDescending(b => b.SubmittedTS).ToArray();
                }
            }
            return View(ret);
        }

        public ActionResult Retry(int id)
        {
            using (var context = new BatchContext())
            {
                var batch = context.Batches.Find(id);
                if (batch != null && (batch.Status == BatchStatus.Completed || batch.Status == BatchStatus.Notified))
                {
                    batch.EnqueueAllAfterExtraction(context);
                    return Json("Success.", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Unable to retry", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                EnqueueFile(file.InputStream, file.FileName, file.ContentLength, User.Identity.Name);
            }
            return Json("All files have been successfully stored.");
        }

        public void EnqueueFile(System.IO.Stream stream, string name, int length, string userName)
        {
            using (var context = new BatchContext())
            {
                var user = GetUser(context, userName);

                // Now we have a user object
                var batch = new Batch()
                {
                    Size = length,
                    SourceContents = new byte[length],
                    Status = BatchStatus.Created,
                    FileName = name,
                    SubmittedTS = DateTime.Now,
                    User = user
                };
                stream.Read(batch.SourceContents, 0, length);
                context.Batches.Add(batch);
                context.SaveChanges();

                var extractionQueue = CprBroker.Engine.Queues.Queue.GetQueues<Queues.ExtractionQueue>().Single();
                extractionQueue.Enqueue(new Queues.BatchQueueItem() { BatchId = batch.BatchId });
            }
        }

        User GetUser(BatchContext context, string userName)
        {
            var user = context.Users.Where(u => u.Name.Equals(userName)).FirstOrDefault();
            if (user == null)
            {
                lock ("User-adding")
                {
                    user = context.Users.Where(u => u.Name.Equals(userName)).FirstOrDefault();
                    if (user == null)
                    {
                        user = new Models.User() { Name = userName };
                        context.Users.Add(user);
                        context.SaveChanges();

                        user = context.Users.Where(u => u.Name.Equals(userName)).FirstOrDefault();
                        context.Entry<User>(user).Collection(u => u.Batches).Load();
                    }
                }
            }
            return user;
        }

        public ActionResult Result(int id)
        {
            using (var context = new BatchContext())
            {
                var batch = context.Batches.Find(id);
                if (batch.GeneratedContents != null)
                    return new FileContentResult(batch.GeneratedContents, "application/csv") { FileDownloadName = string.Format("{0}-result.csv", batch.FileName) };
            }
            return new HttpNotFoundResult();
        }
    }
}