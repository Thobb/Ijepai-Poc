namespace Ijepai.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
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
            
            DropColumn("dbo.LabCreates", "predefinedSoftwares");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LabCreates", "predefinedSoftwares", c => c.String());
            DropForeignKey("dbo.PredefinedSoftwares", "Name", "dbo.LabCreates");
            DropIndex("dbo.PredefinedSoftwares", new[] { "Name" });
            DropTable("dbo.PredefinedSoftwares");
        }
    }
}
