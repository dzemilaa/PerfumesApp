using loginservice.Models;
using Microsoft.EntityFrameworkCore;

namespace loginservice.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users", schema: "public");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                      .HasColumnName("id");

                entity.Property(u => u.Username)
                      .HasColumnName("username")
                      .IsRequired();

                entity.Property(u => u.Email)
                      .HasColumnName("email")
                      .IsRequired();

                entity.Property(u => u.PasswordHash)
                      .HasColumnName("password_hash")
                      .IsRequired();

                entity.Property(u => u.IsActive)
                      .HasColumnName("is_active")
                      .HasDefaultValue(true);

                entity.Property(u => u.IsVerified)
                      .HasColumnName("is_verified")
                      .HasDefaultValue(false);

                entity.Property(u => u.VerificationToken)
                      .HasColumnName("verification_token");
            });
        }
    }
}
