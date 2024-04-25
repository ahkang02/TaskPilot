namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeFeaturesTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PermissionFeatures", "Permission_Id", "dbo.Permissions");
            DropForeignKey("dbo.PermissionFeatures", "Feature_Id", "dbo.Features");
            DropIndex("dbo.PermissionFeatures", new[] { "Permission_Id" });
            DropIndex("dbo.PermissionFeatures", new[] { "Feature_Id" });
            DropTable("dbo.Features");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PermissionFeatures",
                c => new
                    {
                        Permission_Id = c.Guid(nullable: false),
                        Feature_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Permission_Id, t.Feature_Id });
            
            CreateTable(
                "dbo.Features",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Category = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.PermissionFeatures", "Feature_Id");
            CreateIndex("dbo.PermissionFeatures", "Permission_Id");
            AddForeignKey("dbo.PermissionFeatures", "Feature_Id", "dbo.Features", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PermissionFeatures", "Permission_Id", "dbo.Permissions", "Id", cascadeDelete: true);
        }
    }
}
