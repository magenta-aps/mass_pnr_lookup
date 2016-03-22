using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mass_pnr_lookup.Models;

namespace mass_pnr_lookup.Controllers
{
    [RoutePrefix("Admin")]
    public class AdminFilesController : FilesController
    {
        protected override IQueryable<Batch> LoadBatches(BatchContext context)
        {
            return context.Batches;
        }
    }
}