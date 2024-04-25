namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesToDBTask : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Notifications", "TaskId", c => c.Guid());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notifications", "TaskId", c => c.Guid(nullable: false));
        }
    }
}
