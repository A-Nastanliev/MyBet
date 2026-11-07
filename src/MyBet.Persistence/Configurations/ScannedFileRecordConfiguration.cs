using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBet.Persistence.Models;

namespace MyBet.Persistence.Configurations
{
    public class ScannedFileRecordConfiguration : IEntityTypeConfiguration<ScannedFileRecord>
    {
        public void Configure(EntityTypeBuilder<ScannedFileRecord> builder)
        {
            builder.ToTable("ScannedFiles");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Hash).IsRequired().HasMaxLength(64);
            builder.HasIndex(x => x.Hash).IsUnique();

            builder.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();
        }
    }
}
