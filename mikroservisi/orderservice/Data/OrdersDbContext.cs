using orderservice.Models;
using Microsoft.EntityFrameworkCore;

namespace orderservice.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
            : base(options)
        {
        }

        public DbSet<Orders> Orders => Set<Orders>();
        public DbSet<OrderItems> OrderItems => Set<OrderItems>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("orders");

                entity.HasKey(o => o.Id);

                entity.Property(o => o.Id)
                      .HasColumnName("id");

                entity.Property(o => o.UserId)
                      .HasColumnName("userid")
                      .IsRequired();

                entity.Property(o => o.OrderDate)
                      .HasColumnName("orderdate")
                      .IsRequired();

                entity.Property(o => o.Status)
                      .HasColumnName("status")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(o => o.TotalAmount)
                      .HasColumnName("totalamount")
                      .HasColumnType("numeric(18,2)")
                      .IsRequired();

                entity.Property(o => o.FirstName)
                      .HasColumnName("firstname");

                entity.Property(o => o.LastName)
                      .HasColumnName("lastname");

                entity.Property(o => o.Email)
                      .HasColumnName("email");

                entity.Property(o => o.Phone)
                      .HasColumnName("phone");

                entity.Property(o => o.Address)
                      .HasColumnName("address");

                entity.Property(o => o.PromoCode)
                    .HasColumnName("promo_code");

                entity.Property(o => o.Image)
                      .HasColumnName("image");

                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Orders)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.ToTable("orderitems");

                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.Id)
                      .HasColumnName("id");

                entity.Property(oi => oi.ProductId)
                      .HasColumnName("productid")
                      .IsRequired();

                entity.Property(oi => oi.OrderId)
                      .HasColumnName("orderid")
                      .IsRequired();

                entity.Property(oi => oi.Price)
                      .HasColumnName("price")
                      .HasColumnType("numeric(18,2)")
                      .IsRequired();

                entity.Property(oi => oi.Quantity)
                      .HasColumnName("quantity")
                      .IsRequired();

                entity.Property(oi => oi.Image)
                      .HasColumnName("image");
            });
        }

    }
}