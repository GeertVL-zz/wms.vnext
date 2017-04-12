using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductAPI.Model;

namespace ProductAPI.InfraStructure 
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
            
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>(ConfigureProduct);
            builder.Entity<ProductBrand>(ConfigureProductBrand);
            builder.Entity<ProductType>(ConfigureProductType);
        }

        void ConfigureProduct(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Product");

            builder.Property(p => p.Id).ForSqlServerUseSequenceHiLo("product_hilo").IsRequired();
            builder.Property(p => p.Name).IsRequired(true).HasMaxLength(50);
            builder.Property(p => p.Price).IsRequired(true);
            builder.HasOne(p => p.ProductBrand).WithMany().HasForeignKey(p => p.ProductBrandId);
            builder.HasOne(p => p.ProductType).WithMany().HasForeignKey(p => p.ProductTypeId);
        }

        void ConfigureProductBrand(EntityTypeBuilder<ProductBrand> builder)
        {
            builder.ToTable("ProductBrand");
            builder.HasKey(pb => pb.Id);
            builder.Property(pb => pb.Id).ForSqlServerUseSequenceHiLo("product_brand_hilo").IsRequired();
            builder.Property(pb => pb.Brand).IsRequired().HasMaxLength(100);
        }

        void ConfigureProductType(EntityTypeBuilder<ProductType> builder)
        {
            builder.ToTable("ProductType");
            builder.HasKey(pt => pt.Id);
            builder.Property(pt => pt.Id).ForSqlServerUseSequenceHiLo("product_type_hilo").IsRequired();
            builder.Property(pt => pt.Type).IsRequired().HasMaxLength(100);
        }
    }
}