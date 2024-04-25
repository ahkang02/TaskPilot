namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addbackFeaturesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Features",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Permissions", "featuresId", c => c.Guid(nullable: true));
            CreateIndex("dbo.Permissions", "featuresId");
            AddForeignKey("dbo.Permissions", "featuresId", "dbo.Features", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Permissions", "featuresId", "dbo.Features");
            DropIndex("dbo.Permissions", new[] { "featuresId" });
            DropColumn("dbo.Permissions", "featuresId");
            DropTable("dbo.Features");
        }
    }
}
