using Microsoft.EntityFrameworkCore;
using TeamManage.Domain.Entities;

namespace TeamManage.Infrastructure.Persistence;

public class TeamManageDbContext : DbContext
{
    public TeamManageDbContext(DbContextOptions<TeamManageDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamPlayer> TeamPlayers => Set<TeamPlayer>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeamManageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
