namespace Ijepai.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Billings",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Item = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LabID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Labs", t => t.LabID, cascadeDelete: true)
                .Index(t => t.LabID);
            
            CreateTable(
                "dbo.Labs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Time_Zone = c.String(),
                        Start_Time = c.DateTime(nullable: false),
                        End_Time = c.DateTime(nullable: false),
                        Status = c.String(),
                        HKey = c.String(),
                        ApplicationUserID = c.String(maxLength: 128),
                        LabConfigurationID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .Index(t => t.ApplicationUserID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        First_Name = c.String(),
                        Last_Name = c.String(),
                        Email_Address = c.String(),
                        Credit_Card_Number = c.String(),
                        OrganizationID = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationID, cascadeDelete: true)
                .Index(t => t.OrganizationID);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
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
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Organizations",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        OrganizationName = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.LabSoftwareCustoms",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Software_Path = c.String(),
                        LabID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Labs", t => t.LabID, cascadeDelete: true)
                .Index(t => t.LabID);
            
            CreateTable(
                "dbo.LabConfigurations",
                c => new
                    {
                        LabID = c.Int(nullable: false),
                        VM_Count = c.Int(),
                        VM_Type = c.String(),
                        Hard_Disk = c.Double(nullable: false),
                        OS = c.String(),
                        Networked = c.String(),
                    })
                .PrimaryKey(t => t.LabID)
                .ForeignKey("dbo.Labs", t => t.LabID)
                .Index(t => t.LabID);
            
            CreateTable(
                "dbo.LabParticipants",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Email_Address = c.String(),
                        First_Name = c.String(),
                        Last_Name = c.String(),
                        Role = c.String(),
                        VM_ID = c.String(),
                        LabID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Labs", t => t.LabID, cascadeDelete: true)
                .Index(t => t.LabID);
            
            CreateTable(
                "dbo.LabSoftwarePredefineds",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SoftwareID = c.Int(nullable: false),
                        LabID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Labs", t => t.LabID, cascadeDelete: true)
                .ForeignKey("dbo.Softwares", t => t.SoftwareID, cascadeDelete: true)
                .Index(t => t.LabID)
                .Index(t => t.SoftwareID);
            
            CreateTable(
                "dbo.Softwares",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Software_Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.LabCreates",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Time_Zone = c.String(),
                        Start_Time = c.DateTime(nullable: false),
                        End_Time = c.DateTime(nullable: false),
                        VM_Count = c.Int(nullable: false),
                        Networked = c.String(),
                        VM_Type = c.String(nullable: false),
                        Machine_Size = c.Double(nullable: false),
                        OS = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.Participants",
                c => new
                    {
                        Lab_Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false),
                        First_Name = c.String(),
                        Last_Name = c.String(),
                        Role = c.String(),
                        LabCreate_Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Lab_Id)
                .ForeignKey("dbo.LabCreates", t => t.LabCreate_Name)
                .Index(t => t.LabCreate_Name);
            
            CreateTable(
                "dbo.PredefinedSoftwares",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.LabCreates", t => t.Name)
                .Index(t => t.Name);
            
            CreateTable(
                "dbo.LabDataDisks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DataDisk_Path = c.String(),
                        LabID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Labs", t => t.LabID, cascadeDelete: true)
                .Index(t => t.LabID);
            
            CreateTable(
                "dbo.LabFiles",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        File_Path = c.String(),
                        LabID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Labs", t => t.LabID, cascadeDelete: true)
                .Index(t => t.LabID);
            
            CreateTable(
                "dbo.LabVMs",
                c => new
                    {
                        Lab_ID = c.Int(nullable: false),
                        VM_Path = c.String(),
                    })
                .PrimaryKey(t => t.Lab_ID)
                .ForeignKey("dbo.Labs", t => t.Lab_ID)
                .Index(t => t.Lab_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LabVMs", "Lab_ID", "dbo.Labs");
            DropForeignKey("dbo.LabFiles", "LabID", "dbo.Labs");
            DropForeignKey("dbo.LabDataDisks", "LabID", "dbo.Labs");
            DropForeignKey("dbo.PredefinedSoftwares", "Name", "dbo.LabCreates");
            DropForeignKey("dbo.Participants", "LabCreate_Name", "dbo.LabCreates");
            DropForeignKey("dbo.LabSoftwarePredefineds", "SoftwareID", "dbo.Softwares");
            DropForeignKey("dbo.LabSoftwarePredefineds", "LabID", "dbo.Labs");
            DropForeignKey("dbo.LabParticipants", "LabID", "dbo.Labs");
            DropForeignKey("dbo.LabConfigurations", "LabID", "dbo.Labs");
            DropForeignKey("dbo.LabSoftwareCustoms", "LabID", "dbo.Labs");
            DropForeignKey("dbo.Billings", "LabID", "dbo.Labs");
            DropForeignKey("dbo.AspNetUsers", "OrganizationID", "dbo.Organizations");
            DropForeignKey("dbo.Labs", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.LabVMs", new[] { "Lab_ID" });
            DropIndex("dbo.LabFiles", new[] { "LabID" });
            DropIndex("dbo.LabDataDisks", new[] { "LabID" });
            DropIndex("dbo.PredefinedSoftwares", new[] { "Name" });
            DropIndex("dbo.Participants", new[] { "LabCreate_Name" });
            DropIndex("dbo.LabSoftwarePredefineds", new[] { "SoftwareID" });
            DropIndex("dbo.LabSoftwarePredefineds", new[] { "LabID" });
            DropIndex("dbo.LabParticipants", new[] { "LabID" });
            DropIndex("dbo.LabConfigurations", new[] { "LabID" });
            DropIndex("dbo.LabSoftwareCustoms", new[] { "LabID" });
            DropIndex("dbo.Billings", new[] { "LabID" });
            DropIndex("dbo.AspNetUsers", new[] { "OrganizationID" });
            DropIndex("dbo.Labs", new[] { "ApplicationUserID" });
            DropIndex("dbo.AspNetUserClaims", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropTable("dbo.LabVMs");
            DropTable("dbo.LabFiles");
            DropTable("dbo.LabDataDisks");
            DropTable("dbo.PredefinedSoftwares");
            DropTable("dbo.Participants");
            DropTable("dbo.LabCreates");
            DropTable("dbo.Softwares");
            DropTable("dbo.LabSoftwarePredefineds");
            DropTable("dbo.LabParticipants");
            DropTable("dbo.LabConfigurations");
            DropTable("dbo.LabSoftwareCustoms");
            DropTable("dbo.Organizations");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Labs");
            DropTable("dbo.Billings");
        }
    }
}
