using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using ParkingLot.Core.Entities;

namespace ParkingLot.Infrastructure.Data.Configurations
{
    public class ParkingLotConfiguration : IEntityTypeConfiguration<Core.Entities.ParkingLot>
    {
        public void Configure(EntityTypeBuilder<Core.Entities.ParkingLot> builder)
        {
            builder.HasKey(e => e.Id);
        
            builder.Property(e => e.BaseHourlyRate)
                   .HasPrecision(18, 2);
        
            builder.Property(e => e.Images)
                   .HasConversion(
                       v => JsonConvert.SerializeObject(v),
                       v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>()
                   );

            builder.Property(e => e.SlotTypeCapacity)
                   .HasConversion(
                       v => JsonConvert.SerializeObject(v),
                       v => JsonConvert.DeserializeObject<Dictionary<string, int>>(v) ?? new()
                   );

            builder.Property(e => e.HourlyRateByType)
                   .HasConversion(
                       v => JsonConvert.SerializeObject(v),
                       v => JsonConvert.DeserializeObject<Dictionary<string, decimal>>(v) ?? new()
                   );
        }
    }
}
