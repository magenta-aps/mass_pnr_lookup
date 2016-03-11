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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            IEnumerable<Batch> ret;
            using (var context = new Models.BatchContext())
            {
                var user = GetUser(context);
                ret = user.Batches
                    .OrderByDescending(b => b.SubmittedTS).ToArray();
            }
            return View(ret);
        }

        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                using (var context = new BatchContext())
                {
                    var user = GetUser(context);

                    // Now we have a user object
                    var batch = new Batch()
                    {
                        Size = file.ContentLength,
                        SourceContents = new byte[file.ContentLength],
                        Status = BatchStatus.Created,
                        FileName = file.FileName,
                        SubmittedTS = DateTime.Now,
                        User = user
                    };
                    file.InputStream.Read(batch.SourceContents, 0, file.ContentLength);
                    context.Batches.Add(batch);
                    context.SaveChanges();

                    var extractionQueue = CprBroker.Engine.Queues.Queue.GetQueues<Queues.ExtractionQueue>().Single();
                    extractionQueue.Enqueue(new Queues.BatchQueueItem() { BatchId = batch.BatchId });
                }
            }
            return Json("All files have been successfully stored.");
        }

        User GetUser(BatchContext context)
        {
            var user = context.Users.Where(u => u.Name.Equals(User.Identity.Name)).FirstOrDefault();
            if (user == null)
            {
                lock ("User-adding")
                {
                    user = context.Users.Where(u => u.Name.Equals(User.Identity.Name)).FirstOrDefault();
                    if (user == null)
                    {
                        user = new Models.User() { Name = User.Identity.Name };
                        context.Users.Add(user);
                        context.SaveChanges();
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
                    return new FileContentResult(batch.GeneratedContents, "application/csv");
            }
            return new HttpNotFoundResult();
        }
    }
}