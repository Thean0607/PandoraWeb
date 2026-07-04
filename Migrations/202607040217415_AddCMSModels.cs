namespace PandoraWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCMSModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Banners",
                c => new
                    {
                        BannerId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        ImageUrl = c.String(maxLength: 500),
                        LinkUrl = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BannerId);

            CreateTable(
                "dbo.BlogPosts",
                c => new
                    {
                        PostId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 255),
                        ImageUrl = c.String(maxLength: 500),
                        Content = c.String(nullable: false),
                        Author = c.String(maxLength: 100),
                        IsPublished = c.Boolean(nullable: false),
                        PublishedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PostId);

            CreateTable(
                "dbo.Faqs",
                c => new
                    {
                        FaqId = c.Int(nullable: false, identity: true),
                        Question = c.String(nullable: false, maxLength: 500),
                        Answer = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FaqId);

            CreateTable(
                "dbo.Pages",
                c => new
                    {
                        PageId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Slug = c.String(nullable: false, maxLength: 200),
                        Content = c.String(),
                        IsPublished = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PageId);
        }

        public override void Down()
        {
            DropTable("dbo.Pages");
            DropTable("dbo.Faqs");
            DropTable("dbo.BlogPosts");
            DropTable("dbo.Banners");
        }
    }
}
