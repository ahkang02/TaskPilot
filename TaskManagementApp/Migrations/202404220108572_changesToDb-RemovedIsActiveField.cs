namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesToDbRemovedIsActiveField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Permissions", "IsActive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Permissions", "IsActive", c => c.Boolean(nullable: false));
        }
    }
}
