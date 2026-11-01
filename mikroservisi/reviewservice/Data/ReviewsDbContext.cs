using Microsoft.EntityFrameworkCore;
using reviewservice.Models;

namespace reviewservice.Data
{
  
        public class ReviewsDbContext : DbContext
        {
            public ReviewsDbContext(DbContextOptions<ReviewsDbContext> options)
                : base(options)
            {
            }

            public DbSet<Reviews> Reviews { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Reviews>(entity =>
                {
                    entity.ToTable("reviews");
                    entity.HasKey(e => e.ReviewId);
                    entity.Property(e => e.ReviewId).HasColumnName("reviewid");
                    entity.Property(e => e.OrderId).HasColumnName("orderid");
                    entity.Property(e => e.UserId).HasColumnName("userid");
                    entity.Property(e => e.ProductId).HasColumnName("productid");
                    entity.Property(e => e.ReviewText).HasColumnName("reviewtext");
                    entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                });
            }
        }
    }

