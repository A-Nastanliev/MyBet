using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBet.Persistence.Models;

namespace MyBet.Persistence.Configurations
{
    public class ReviewItemRecordConfiguration : IEntityTypeConfiguration<ReviewItemRecord>
    {
        public void Configure(EntityTypeBuilder<ReviewItemRecord> builder)
        {
            builder.ToTable("ReviewQueue");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileHash).IsRequired().HasMaxLength(64);
            builder.HasIndex(x => x.FileHash).IsUnique();

            builder.Property(x => x.FilePath).IsRequired();

            builder.Property(x => x.SlipId).IsRequired();
        }
    }
}
