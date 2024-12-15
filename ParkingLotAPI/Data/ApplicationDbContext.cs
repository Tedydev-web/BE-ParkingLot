using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingLotAPI.Models;
using ParkingLotAPI.Models.Auth;

namespace ParkingLotAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingLot> ParkingLots { get; set; }
        public DbSet<ParkingLotImage> ParkingLotImages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingLot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.PricePerHour).HasColumnType("decimal(18,2)");
                
                entity.Property(e => e.Types)
                    .IsRequired()
                    .HasDefaultValue("parking");
            });

            modelBuilder.Entity<ParkingLotImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ParkingLot)
                      .WithMany(p => p.Images)
                      .HasForeignKey(e => e.ParkingLotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.JwtId).IsRequired();
                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId);
            });
        }
    }
}
