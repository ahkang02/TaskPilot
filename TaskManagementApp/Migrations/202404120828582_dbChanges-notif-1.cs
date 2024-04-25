namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbChangesnotif1 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Notifications", name: "Tasks_Id", newName: "TasksId");
            RenameIndex(table: "dbo.Notifications", name: "IX_Tasks_Id", newName: "IX_TasksId");
            DropColumn("dbo.Notifications", "TaskId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "TaskId", c => c.Guid());
            RenameIndex(table: "dbo.Notifications", name: "IX_TasksId", newName: "IX_Tasks_Id");
            RenameColumn(table: "dbo.Notifications", name: "TasksId", newName: "Tasks_Id");
        }
    }
}
