namespace mass_pnr_lookup.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Batches",
                c => new
                    {
                        BatchId = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        SubmittedTS = c.DateTime(nullable: false),
                        CompletedTS = c.DateTime(),
                        Size = c.Int(nullable: false),
                        FileName = c.String(),
                        SourceContents = c.Binary(),
                        GeneratedContents = c.Binary(),
                        GenerationSemaphoreId = c.Guid(nullable: false),
                        NotificationSemaphoreId = c.Guid(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.BatchId)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.BatchLines",
                c => new
                    {
                        BatchElementId = c.Int(nullable: false, identity: true),
                        SourceContents = c.String(),
                        Row = c.Int(nullable: false),
                        Address = c.String(),
                        Name = c.String(),
                        PNR = c.String(),
                        Error = c.String(),
                        Batch_BatchId = c.Int(),
                    })
                .PrimaryKey(t => t.BatchElementId)
                .ForeignKey("dbo.Batches", t => t.Batch_BatchId)
                .Index(t => t.Batch_BatchId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Batches", "User_Id", "dbo.Users");
            DropForeignKey("dbo.BatchLines", "Batch_BatchId", "dbo.Batches");
            DropIndex("dbo.Users", new[] { "Name" });
            DropIndex("dbo.BatchLines", new[] { "Batch_BatchId" });
            DropIndex("dbo.Batches", new[] { "User_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.BatchLines");
            DropTable("dbo.Batches");
        }
    }
}
