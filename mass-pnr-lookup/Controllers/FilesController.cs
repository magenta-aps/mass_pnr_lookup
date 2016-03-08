﻿using System;
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
            IEnumerable<Batch> ret;
            using (var context = new Models.BatchContext())
            {
                ret = context.Batches.ToArray();
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
                }
            }
            return Json("All files have been successfully stored.");
        }
    }
}