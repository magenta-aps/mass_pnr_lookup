using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mass_pnr_lookup.Models;
using PagedList;
using PagedList.Mvc;

namespace mass_pnr_lookup.Controllers
{
    public class FilesController : Controller
    {
        public ActionResult IndexAll()
        {
            return Index(true);
        }

        public ActionResult Index()
        {
            return Index(false);
        }

        [NonAction]
        public ActionResult Index(bool allUsers)
        {
            return View("Index", allUsers);
        }

        public ActionResult List(int pageNumber = 1, int pageSize = 5, bool allUsers = false)
        {
            IQueryable<Batch> ret;

            using (var context = new Models.BatchContext())
            {
                if (allUsers)
                {
                    ret = context.Batches
                        .OrderByDescending(b => b.SubmittedTS);
                }
                else
                {
                    var user = GetUser(context, User.Identity.Name);

                    ret = user.Batches
                        .OrderByDescending(b => b.SubmittedTS).AsQueryable();
                }
                return View(new PagedList<Batch>(ret, pageNumber, pageSize));
            }
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

        public ActionResult Result(int id)
        {
            using (var context = new BatchContext())
            {
                var batch = context.Batches.Find(id);
                if (batch.GeneratedContents != null)
                    return new FileContentResult(batch.GeneratedContents, "application/txt") { FileDownloadName = string.Format("{0}-result.txt", batch.FileName) };
            }
            return new HttpNotFoundResult();
        }

        #region Utility methods
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
        #endregion
    }
}