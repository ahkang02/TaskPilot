namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbChangesTask : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Tasks", name: "AssignTo_Id", newName: "AssignToId");
            RenameIndex(table: "dbo.Tasks", name: "IX_AssignTo_Id", newName: "IX_AssignToId");
            DropColumn("dbo.Tasks", "AssigneeToId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tasks", "AssigneeToId", c => c.String());
            RenameIndex(table: "dbo.Tasks", name: "IX_AssignToId", newName: "IX_AssignTo_Id");
            RenameColumn(table: "dbo.Tasks", name: "AssignToId", newName: "AssignTo_Id");
        }
    }
}
