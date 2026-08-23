using Microsoft.EntityFrameworkCore;
using MvP.Application.Interfaces;
using MvP.Domain.Entities.Teams;
namespace MvP.Infrastructure.Persistence;
public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext db;
    public TeamRepository(AppDbContext db)
    {
        this.db = db;
    }
    public async Task<IReadOnlyCollection<Team>> GetAllAsync()
    {
        return await db.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
    public async Task<Team?> GetByIdWithMembersAsync(Guid teamId)
    {
        return await db.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }
    public async Task<Team?> GetByIdAsync(Guid teamId)
    {
        return await db.Teams.FindAsync(teamId);
    }
    public async Task<Team?> GetByJoinCodeAsync(string joinCode)
    {
        return await db.Teams.FirstOrDefaultAsync(t => t.JoinCode == joinCode);
    }
    public async Task<TeamMember?> GetMembershipAsync(Guid teamId, Guid userId)
    {
        return await db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
    }
    public async Task AddAsync(Team team)
    {
        await db.Teams.AddAsync(team);
        await db.SaveChangesAsync();
    }
    public async Task AddMemberAsync(TeamMember member)
    {
        await db.TeamMembers.AddAsync(member);
        await db.SaveChangesAsync();
    }
    public async Task RemoveMemberAsync(TeamMember member)
    {
        db.TeamMembers.Remove(member);
        await db.SaveChangesAsync();
    }
    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }



}   
