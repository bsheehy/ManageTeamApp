using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamManage.Domain.Entities;

namespace TeamManage.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(u => u.ManagedTeams)
            .WithOne(t => t.Manager)
            .HasForeignKey(t => t.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
