﻿using System;
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
        #region User actions

        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult List(int pageNumber = 1, int pageSize = 10)
        {
            using (var context = new Models.BatchContext())
            {
                IQueryable<Batch> ret = LoadBatches(context).OrderByDescending(b => b.SubmittedTS);
                return PartialView("../Files/List", new PagedList<Batch>(ret, pageNumber, pageSize));
            }
        }

        public ActionResult ListLines(int id, int pageNumber = 1, int pageSize = 10)
        {
            using (var context = new Models.BatchContext())
            {
                var batch = LoadBatch(id, context);
                if (batch != null)
                {
                    var lines = context.BatchLines.Where(l => l.Batch_BatchId == id).OrderBy(l => l.Row)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToArray();
                    ViewBag.BatchId = id;
                    return PartialView("../Files/ListLines", new StaticPagedList<BatchLine>(lines, pageNumber, pageSize, batch.NumLines));
                }
                return new HttpNotFoundResult();
            }
        }

        public ActionResult Pause(int id)
        {
            using (var context = new BatchContext())
            {
                var batch = LoadBatch(id, context);
                if (batch != null && batch.Status == BatchStatus.Processing)
                {
                    batch.Status = BatchStatus.Paused;
                    context.SaveChanges();
                    batch.SearchSemaphore().Wait();
                    return Json("Success.", JsonRequestBehavior.AllowGet);
                }
            }
            return new HttpNotFoundResult();
        }

        public ActionResult Resume(int id)
        {
            using (var context = new BatchContext())
            {
                var batch = LoadBatch(id, context);
                if (batch != null && batch.Status == BatchStatus.Paused)
                {
                    batch.Status = BatchStatus.Processing;
                    context.SaveChanges();
                    batch.SearchSemaphore().SignalAll();
                    return Json("Success.", JsonRequestBehavior.AllowGet);
                }
            }
            return new HttpNotFoundResult();
        }

        public ActionResult Retry(int id)
        {
            using (var context = new BatchContext())
            {
                var batch = LoadBatch(id, context);

                if (batch != null && (batch.Status == BatchStatus.Completed || batch.Status == BatchStatus.Notified))
                {
                    batch.Status = BatchStatus.Created;
                    batch.ResetCounters();
                    if (batch.Lines != null)
                    {
                        context.BatchLines.RemoveRange(batch.Lines);
                    }
                    context.SaveChanges();

                    var extractionQueue = CprBroker.Engine.Queues.Queue.GetQueues<Queues.ExtractionQueue>().Single();
                    extractionQueue.Enqueue(new Queues.BatchQueueItem() { BatchId = batch.BatchId });

                    return Json("Success.", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Unable to retry", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Remove(int id)
        {
            try
            {
                using (var context = new BatchContext())
                {
                    var batch = LoadBatch(id, context);
                    if (batch != null)
                    {
                        context.Batches.Remove(batch);
                        context.SaveChanges();
                        batch.SignalAllSemaphores();
                        return Json("Success.",JsonRequestBehavior.AllowGet);
                    }
                }
            } catch (Exception)
            {
                return Json("An Error occured.", JsonRequestBehavior.AllowGet);
                // DEBUGGING return Json(ex.Message+"\r\n"+ex.StackTrace+"\r\n\r\n"+ex.InnerException, JsonRequestBehavior.AllowGet);
            }
            return new HttpStatusCodeResult(500);
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
                var batch = LoadBatch(id, context);

                if (batch != null && batch.GeneratedContents != null)
                {
                    var ext = batch.FileName.Split('.').Skip(1).LastOrDefault();
                    ext = string.IsNullOrEmpty(ext) ? "" : "." + ext;
                    return new FileContentResult(batch.GeneratedContents, "application/unspecified")
                    {
                        FileDownloadName = string.Format("{0}-result{1}", batch.FileName, ext)
                    };
                }
            }
            return new HttpNotFoundResult();
        }
        #endregion

        #region Utility methods
        [NonAction]
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

        [NonAction]
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

        #region overridable methods
        protected virtual IQueryable<Batch> LoadBatches(BatchContext context)
        {
            return GetUser(context, User.Identity.Name).Batches.AsQueryable();
        }

        protected Batch LoadBatch(int id, BatchContext context)
        {
            return LoadBatches(context).Where(b => b.BatchId == id).SingleOrDefault();
        }
        #endregion
    }
}