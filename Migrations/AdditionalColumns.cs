namespace JYXWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "UserCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "UserCode");
        }
    }

    public partial class PasswordPlain : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PasswordPlain", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PasswordPlain");
        }
    }

    public partial class FirstName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FirstName");
        }
    }

    public partial class LastName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LastName", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "LastName");
        }
    }
}
