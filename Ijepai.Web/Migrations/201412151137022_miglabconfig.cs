namespace Ijepai.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class miglabconfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LabConfigurations", "Machine_Size", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LabConfigurations", "Machine_Size");
        }
    }
}
