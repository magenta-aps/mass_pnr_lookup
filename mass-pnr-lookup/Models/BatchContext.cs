﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace mass_pnr_lookup.Models
{
    public class BatchContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<BatchLine> BatchLines { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BatchContext, Migrations.Configuration>());
        }

        public BatchContext()
            : base(Properties.Settings.Default.MassPnrLookupConnectionString)
        { }
    }
}