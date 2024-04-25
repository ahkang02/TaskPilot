namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class featureTableAdded : DbMigration
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
            
            CreateTable(
                "dbo.FeaturePermissions",
                c => new
                    {
                        Feature_Id = c.Guid(nullable: false),
                        Permission_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Feature_Id, t.Permission_Id })
                .ForeignKey("dbo.Features", t => t.Feature_Id, cascadeDelete: true)
                .ForeignKey("dbo.Permissions", t => t.Permission_Id, cascadeDelete: true)
                .Index(t => t.Feature_Id)
                .Index(t => t.Permission_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FeaturePermissions", "Permission_Id", "dbo.Permissions");
            DropForeignKey("dbo.FeaturePermissions", "Feature_Id", "dbo.Features");
            DropIndex("dbo.FeaturePermissions", new[] { "Permission_Id" });
            DropIndex("dbo.FeaturePermissions", new[] { "Feature_Id" });
            DropTable("dbo.FeaturePermissions");
            DropTable("dbo.Features");
        }
    }
}
