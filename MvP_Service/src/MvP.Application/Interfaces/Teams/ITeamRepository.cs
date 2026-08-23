using MvP.Domain.Entities.Teams;

namespace MvP.Application.Interfaces;

public interface ITeamRepository
{
    Task<IReadOnlyCollection<Team>> GetAllAsync();
    Task<Team?> GetByIdWithMembersAsync(Guid teamId);
    Task<Team?> GetByIdAsync(Guid teamId);
    Task<Team?> GetByJoinCodeAsync(string joinCode);
    Task<TeamMember?> GetMembershipAsync(Guid teamId, Guid userId);
    Task AddAsync(Team team);
    Task AddMemberAsync(TeamMember member);
    Task RemoveMemberAsync(TeamMember member);
    Task SaveChangesAsync();
}
