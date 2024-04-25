namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDependencyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tasks", "DependencyId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tasks", "DependencyId");
        }
    }
}
