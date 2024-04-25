namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class featureTableModify : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FeaturePermissions", newName: "PermissionFeatures");
            DropPrimaryKey("dbo.PermissionFeatures");
            AddColumn("dbo.Features", "Category", c => c.String());
            AddPrimaryKey("dbo.PermissionFeatures", new[] { "Permission_Id", "Feature_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.PermissionFeatures");
            DropColumn("dbo.Features", "Category");
            AddPrimaryKey("dbo.PermissionFeatures", new[] { "Feature_Id", "Permission_Id" });
            RenameTable(name: "dbo.PermissionFeatures", newName: "FeaturePermissions");
        }
    }
}
