using System.Data.Entity;
using Ecommerce.Core.Entities;

namespace Ecommerce.Data
{
    /// <summary>
    /// EF6 context for LegacyEcommerceDb. The visual model lives in LegacyEcommerce.edmx
    /// (same folder; build action EntityDeploy embeds the csdl/ssdl/msl metadata, design-time
    /// only — refresh via "Update Model from Database…" after database\
    /// 01_Create_LegacyEcommerceDb.sql has been approved and run). The runtime mapping is the
    /// fluent configuration below, mirroring that SQL script table-for-table, so the
    /// application builds and runs against the script schema. Connection string "EcommerceDb"
    /// (see Ecommerce.Web\Web.config). Database initializers are disabled: the schema is
    /// owned by the SQL script, never by EF.
    /// </summary>
    public class EcommerceDbContext : DbContext
    {
        static EcommerceDbContext()
        {
            Database.SetInitializer<EcommerceDbContext>(null);
        }

        public EcommerceDbContext()
            : base("name=EcommerceDb")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public EcommerceDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<ProductVariant> ProductVariants { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ---- Categories (self FK: ParentCategoryId -> Categories.Id) ----
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId);

            // ---- Products ----
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // ---- ProductImages ----
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");
            modelBuilder.Entity<ProductImage>().HasKey(i => i.Id);
            modelBuilder.Entity<ProductImage>()
                .HasRequired(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .WillCascadeOnDelete(true);

            // ---- ProductVariants ----
            modelBuilder.Entity<ProductVariant>().ToTable("ProductVariants");
            modelBuilder.Entity<ProductVariant>().HasKey(v => v.Id);
            modelBuilder.Entity<ProductVariant>().Property(v => v.PriceAdjustment).HasPrecision(18, 2);
            modelBuilder.Entity<ProductVariant>()
                .HasRequired(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .WillCascadeOnDelete(true);

            // ---- Customers (UserId is a plain scalar FK to AspNetUsers, no navigation) ----
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Customer>().HasKey(c => c.Id);

            // ---- Addresses ----
            modelBuilder.Entity<Address>().ToTable("Addresses");
            modelBuilder.Entity<Address>().HasKey(a => a.Id);
            modelBuilder.Entity<Address>()
                .HasRequired(a => a.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CustomerId)
                .WillCascadeOnDelete(true);

            // ---- CartItems (UserId plain scalar; optional Variant) ----
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<CartItem>().HasKey(c => c.Id);
            modelBuilder.Entity<CartItem>().Property(c => c.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<CartItem>()
                .HasRequired(c => c.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(c => c.ProductId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<CartItem>()
                .HasOptional(c => c.ProductVariant)
                .WithMany(v => v.CartItems)
                .HasForeignKey(c => c.VariantId)
                .WillCascadeOnDelete(false);

            // ---- Orders ----
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<Order>().Property(o => o.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.ShippingCost).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.Tax).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .HasOptional(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .WillCascadeOnDelete(false);

            // ---- OrderLines ----
            modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            modelBuilder.Entity<OrderLine>().HasKey(l => l.Id);
            modelBuilder.Entity<OrderLine>().Property(l => l.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<OrderLine>().Property(l => l.LineTotal).HasPrecision(18, 2);
            modelBuilder.Entity<OrderLine>()
                .HasRequired(l => l.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(l => l.OrderId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<OrderLine>()
                .HasRequired(l => l.Product)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(l => l.ProductId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
