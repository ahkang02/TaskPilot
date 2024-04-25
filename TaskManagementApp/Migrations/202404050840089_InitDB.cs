namespace TaskManagementApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Description = c.String(),
                        Status = c.String(),
                        TaskId = c.Guid(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        Tasks_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.Tasks", t => t.Tasks_Id)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Tasks_Id);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        PriorityId = c.Guid(nullable: false),
                        StatusId = c.Guid(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        Created = c.DateTime(nullable: false),
                        AssigneeToId = c.String(),
                        AssignFromId = c.String(maxLength: 128),
                        AssignTo_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AssignFromId)
                .ForeignKey("dbo.AspNetUsers", t => t.AssignTo_Id)
                .ForeignKey("dbo.Priorities", t => t.PriorityId, cascadeDelete: true)
                .ForeignKey("dbo.Statuses", t => t.StatusId, cascadeDelete: true)
                .Index(t => t.PriorityId)
                .Index(t => t.StatusId)
                .Index(t => t.AssignFromId)
                .Index(t => t.AssignTo_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                        LastLogin = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Priorities",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Description = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Statuses",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Description = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Permissions",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        IsActive = c.Boolean(),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.RolesPermissions",
                c => new
                    {
                        Roles_Id = c.String(nullable: false, maxLength: 128),
                        Permission_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Roles_Id, t.Permission_Id })
                .ForeignKey("dbo.AspNetRoles", t => t.Roles_Id, cascadeDelete: true)
                .ForeignKey("dbo.Permissions", t => t.Permission_Id, cascadeDelete: true)
                .Index(t => t.Roles_Id)
                .Index(t => t.Permission_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.RolesPermissions", "Permission_Id", "dbo.Permissions");
            DropForeignKey("dbo.RolesPermissions", "Roles_Id", "dbo.AspNetRoles");
            DropForeignKey("dbo.Notifications", "Tasks_Id", "dbo.Tasks");
            DropForeignKey("dbo.Tasks", "StatusId", "dbo.Statuses");
            DropForeignKey("dbo.Tasks", "PriorityId", "dbo.Priorities");
            DropForeignKey("dbo.Tasks", "AssignTo_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tasks", "AssignFromId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Notifications", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.RolesPermissions", new[] { "Permission_Id" });
            DropIndex("dbo.RolesPermissions", new[] { "Roles_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Tasks", new[] { "AssignTo_Id" });
            DropIndex("dbo.Tasks", new[] { "AssignFromId" });
            DropIndex("dbo.Tasks", new[] { "StatusId" });
            DropIndex("dbo.Tasks", new[] { "PriorityId" });
            DropIndex("dbo.Notifications", new[] { "Tasks_Id" });
            DropIndex("dbo.Notifications", new[] { "ApplicationUser_Id" });
            DropTable("dbo.RolesPermissions");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Permissions");
            DropTable("dbo.Statuses");
            DropTable("dbo.Priorities");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Tasks");
            DropTable("dbo.Notifications");
        }
    }
}
