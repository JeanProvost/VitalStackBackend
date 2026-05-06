using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Backend.Core.Entities.Supplement.Persistence

{
    public class SupplementConfiguration : IEntityTypeConfiguration<Supplement>
    {
        public void Configure(EntityTypeBuilder<Supplement> builder)
        {
            builder.Metadata.SetAnnotation("Relational:TableName", "Supplements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Form)
                .IsRequired();

            builder.Property(x => x.DosageAmount)
                .IsRequired();

            builder.Property(x => x.DosageUnit)
                .IsRequired();

            builder.Property(x => x.TimeOfDay)
                .IsRequired();

            //Constraints and Index
            builder.HasIndex(e => new { e.Name, e.Form }).IsUnique();
        }
    }
}
