using Microsoft.EntityFrameworkCore;
using shopservice.Models;

namespace ShopService.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options) { }

    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<FavoriteItem> FavoriteItems { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Cart");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id)
                  .HasColumnName("Id")
                  .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(c => c.UserId).HasColumnName("UserId").IsRequired();

            entity.HasMany(c => c.Items)
                  .WithOne(i => i.Cart)
                  .HasForeignKey(i => i.CartId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Id)
                  .HasColumnName("Id")
                  .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(ci => ci.CartId).HasColumnName("CartId").IsRequired();
            entity.Property(ci => ci.ProductId).HasColumnName("ProductId").IsRequired();
            entity.Property(ci => ci.Quantity).HasColumnName("Quantity").IsRequired();
            entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("Favorite");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Id)
                  .HasColumnName("Id")
                  .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(f => f.UserId).HasColumnName("UserId").IsRequired();

            entity.HasMany(f => f.Items)
                  .WithOne(i => i.Favorite)
                  .HasForeignKey(i => i.FavoriteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FavoriteItem>(entity =>
        {
            entity.ToTable("FavoriteItems");
            entity.HasKey(fi => fi.Id);
            entity.Property(fi => fi.Id)
                  .HasColumnName("Id")
                  .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(fi => fi.FavoriteId).HasColumnName("FavoriteId").IsRequired();
            entity.Property(fi => fi.ProductId).HasColumnName("ProductId").IsRequired();
        });
    }
}