using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBet.Persistence.Models;

namespace MyBet.Persistence.Configurations
{
    public class SlipRecordConfiguration : IEntityTypeConfiguration<SlipRecord>
    {
        public void Configure(EntityTypeBuilder<SlipRecord> builder)
        {
            builder.ToTable("Slips");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SlipId).IsRequired().UseCollation("BINARY");
            builder.HasIndex(x => x.SlipId).IsUnique();

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        }
    }
}
