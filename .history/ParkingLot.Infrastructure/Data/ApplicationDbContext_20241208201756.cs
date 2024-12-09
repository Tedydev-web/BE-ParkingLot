using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using ParkingLot.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace ParkingLot.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Core.Entities.ParkingLot> ParkingLots { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Core.Entities.ParkingLot>(entity =>
            {
                // Cấu hình primary key
                entity.HasKey(e => e.Id);
                
                // Cấu hình precision cho decimal
                entity.Property(e => e.BaseHourlyRate)
                      .HasPrecision(18, 2);

                // Cấu hình relationships
                entity.HasOne(e => e.Owner)
                      .WithMany(u => u.OwnedParkingLots)
                      .HasForeignKey(e => e.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Cấu hình JSON conversion cho Dictionary
                entity.Property(e => e.SlotTypeCapacity)
                      .HasConversion(
                          v => JsonConvert.SerializeObject(v ?? new()),
                          v => JsonConvert.DeserializeObject<Dictionary<string, int>>(v) ?? new()
                      );

                entity.Property(e => e.HourlyRateByType)
                      .HasConversion(
                          v => JsonConvert.SerializeObject(v ?? new()),
                          v => JsonConvert.DeserializeObject<Dictionary<string, decimal>>(v) ?? new()
                      );

                // Cấu hình collection navigation properties
                entity.HasMany(e => e.PriceHistory)
                      .WithOne(p => p.ParkingLot)
                      .HasForeignKey(p => p.ParkingLotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ParkingSlot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ParkingLot)
                      .WithMany(p => p.ParkingSlots)
                      .HasForeignKey(e => e.ParkingLotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PriceHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PricesByVehicleType)
                      .HasConversion(
                          v => JsonConvert.SerializeObject(v ?? new Dictionary<string, decimal>()),
                          v => JsonConvert.DeserializeObject<Dictionary<string, decimal>>(v) ?? 
                               new Dictionary<string, decimal>()
                      )
                      .Metadata.SetValueComparer(
                          new ValueComparer<Dictionary<string, decimal>>(
                              (c1, c2) => (c1 ?? new Dictionary<string, decimal>()).SequenceEqual(c2 ?? new Dictionary<string, decimal>()),
                              c => (c ?? new Dictionary<string, decimal>()).Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c => new Dictionary<string, decimal>(c ?? new Dictionary<string, decimal>())
                          ));

                entity.HasOne(e => e.ParkingLot)
                      .WithMany(p => p.PriceHistory)
                      .HasForeignKey(e => e.ParkingLotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ParkingLot)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(e => e.ParkingLotId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}