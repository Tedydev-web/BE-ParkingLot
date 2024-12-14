using Microsoft.EntityFrameworkCore;
using ParkingLotAPI.Models;

namespace ParkingLotAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingLot> ParkingLots { get; set; }
        public DbSet<ParkingLotImage> ParkingLotImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingLot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.PricePerHour).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<ParkingLotImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ParkingLot)
                      .WithMany(p => p.Images)
                      .HasForeignKey(e => e.ParkingLotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
