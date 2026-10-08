using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamManage.Domain.Entities;

namespace TeamManage.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Opponent).IsRequired().HasMaxLength(200);

        builder.Property(m => m.Location).IsRequired().HasMaxLength(200);

        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(m => m.Selections)
            .WithOne(mp => mp.Match)
            .HasForeignKey(mp => mp.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MatchPlayerConfiguration : IEntityTypeConfiguration<MatchPlayer>
{
    public void Configure(EntityTypeBuilder<MatchPlayer> builder)
    {
        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.AttendanceStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(mp => new { mp.MatchId, mp.PlayerId }).IsUnique();

        builder.HasOne(mp => mp.Player)
            .WithMany(u => u.MatchSelections)
            .HasForeignKey(mp => mp.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mp => mp.Position)
            .WithMany(p => p.MatchPlayers)
            .HasForeignKey(mp => mp.PositionId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
