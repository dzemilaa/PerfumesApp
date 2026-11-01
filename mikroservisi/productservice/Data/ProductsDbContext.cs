using Microsoft.EntityFrameworkCore;
using productservice.Models;

namespace productservice.Data
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
            : base(options)
        {
        }

        public DbSet<products> Products { get; set; }
        public DbSet<Brands> Brands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<products>(entity =>
            {
                entity.ToTable("products"); // tabela u bazi
                //mapiranje id (case-sensitive) na Id u modelu
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.Discount).HasColumnName("discount");
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.BrandId).HasColumnName("brand_id");
                entity.Property(e => e.GenderId).HasColumnName("gender_id");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.Size).HasColumnName("size");
            });
            modelBuilder.Entity<Brands>(entity =>
            {
                entity.ToTable("brands"); 

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });
            
            modelBuilder.Entity<products>()
                .HasOne(p => p.Brands)//jedan proizvod ima jedan brend
                .WithMany(b => b.Products)//jedan brend moze imati vise proizvoda
                .HasForeignKey(p => p.BrandId);

            modelBuilder.Entity<Brands>().HasData(
                new Brands { Id = 1, Name = "Bella Demo" });

            modelBuilder.Entity<products>().HasData(
                new products
                {
                    Id = Guid.Parse("d34817b0-9667-4e51-9dbc-7804d85ddc60"),
                    Name = "Rose Bloom",
                    Description = "A soft floral fragrance for everyday wear.",
                    Price = 49.99m,
                    Discount = 0,
                    Stock = 20,
                    CategoryId = 1,
                    BrandId = 1,
                    GenderId = 1,
                    ImageUrl = "http://localhost:5174/demo-products/perfume1.jpg",
                    Size = 50
                },
                new products
                {
                    Id = Guid.Parse("3f90e4d7-39c1-40d0-9994-8ed2fd156d09"),
                    Name = "Midnight Woods",
                    Description = "A warm woody fragrance with a clean finish.",
                    Price = 59.99m,
                    Discount = 10,
                    Stock = 15,
                    CategoryId = 1,
                    BrandId = 1,
                    GenderId = 2,
                    ImageUrl = "http://localhost:5174/demo-products/perfume2.jpg",
                    Size = 100
                },
                new products
                {
                    Id = Guid.Parse("c886c20f-3b4e-4e38-89b4-7a632f690e8e"),
                    Name = "Citrus Day",
                    Description = "A bright unisex scent with fresh citrus notes.",
                    Price = 39.99m,
                    Discount = 0,
                    Stock = 25,
                    CategoryId = 1,
                    BrandId = 1,
                    GenderId = 3,
                    ImageUrl = "http://localhost:5174/demo-products/perfume3.jpg",
                    Size = 50
                });
        }
    }
}
