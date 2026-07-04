using System.Data.Entity;

namespace PandoraWeb.Models.Data
{
    public class PandoraDbContext : DbContext
    {
        // Truyền tên chuỗi kết nối vào DbContext constructor
        public PandoraDbContext() : base("name=PandoraConnectionString")
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure some relationships to avoid cascade delete cycles
            modelBuilder.Entity<Order>()
                .HasOptional(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
