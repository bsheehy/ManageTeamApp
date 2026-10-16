using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Clubs;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Entities;
using TeamManage.Domain.Enums;

namespace TeamManage.Application.Services;

public class ClubService : IClubService
{
    private readonly IClubRepository _clubs;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public ClubService(IClubRepository clubs, IUserRepository users, IUnitOfWork unitOfWork)
    {
        _clubs = clubs;
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClubDto>> CreateClubAsync(Guid clubManagerId, CreateClubRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<ClubDto>.Failure("Club name is required.");
        }

        var user = await _users.GetByIdAsync(clubManagerId, ct);
        if (user is null || user.Role != UserRole.ClubManager)
        {
            return Result<ClubDto>.Failure("Only club managers can create clubs.");
        }

        var club = new Club { Name = request.Name.Trim(), ClubManagerId = clubManagerId };
        await _clubs.AddAsync(club, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClubDto>.Success(MapToDto(club));
    }

    public async Task<Result<List<ClubDto>>> GetClubsForManagerAsync(Guid clubManagerId, CancellationToken ct = default)
    {
        var clubs = await _clubs.GetByClubManagerIdAsync(clubManagerId, ct);
        return Result<List<ClubDto>>.Success(clubs.Select(MapToDto).ToList());
    }

    private static ClubDto MapToDto(Club club) => new()
    {
        Id = club.Id,
        Name = club.Name,
        TeamCount = club.Teams.Count
    };
}
