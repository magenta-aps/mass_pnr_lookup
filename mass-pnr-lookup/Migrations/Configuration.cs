namespace mass_pnr_lookup.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<mass_pnr_lookup.Models.BatchContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(mass_pnr_lookup.Models.BatchContext context)
        {
            foreach (var batch in context.Batches.Where(b => b.SearchSemaphoreId == Guid.Empty).ToArray())
            {
                batch.SearchSemaphoreId = CprBroker.Engine.Queues.Semaphore.Create().Impl.SemaphoreId;
                batch.SearchSemaphore().Signal();
                context.SaveChanges();
            }

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
