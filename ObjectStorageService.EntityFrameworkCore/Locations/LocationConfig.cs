using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Template.Domain;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Service.Template.Locations;

public class LocationConfig : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable(nameof(Location), ObjectStorageServiceDbProperties.DbSchema);
        builder.ConfigureByConvention();

        builder.HasOne(l => l.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(fk => fk.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
            ;
    }
}
