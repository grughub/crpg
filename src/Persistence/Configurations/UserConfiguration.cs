using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => new { u.Platform, u.PlatformUserId }).IsUnique();

        builder
            .HasOne(u => u.ActiveCharacter)
            .WithOne()
            .HasForeignKey<User>(u => u.ActiveCharacterId);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
