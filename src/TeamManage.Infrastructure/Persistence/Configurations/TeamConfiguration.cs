using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamManage.Domain.Entities;

namespace TeamManage.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);

        builder.Property(t => t.Sport).HasConversion<string>().HasMaxLength(30);

        builder.HasMany(t => t.Players)
            .WithOne(tp => tp.Team)
            .HasForeignKey(tp => tp.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Positions)
            .WithOne(p => p.Team)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Matches)
            .WithOne(m => m.Team)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TeamPlayerConfiguration : IEntityTypeConfiguration<TeamPlayer>
{
    public void Configure(EntityTypeBuilder<TeamPlayer> builder)
    {
        builder.HasKey(tp => tp.Id);

        builder.HasIndex(tp => new { tp.TeamId, tp.PlayerId }).IsUnique();

        builder.HasOne(tp => tp.Player)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(tp => tp.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
    }
}
