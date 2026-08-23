using MvP.Application.DTOs;
using MvP.Domain.Entities.Teams;
using MvP.Domain.Enums.Teams;
using MvP.Application.Interfaces;

namespace MvP.Application.Services.Teams
{
    public class TeamService 
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ICurrentUser _currentUser;

        public TeamService(ITeamRepository teamRepository, ICurrentUser currentUser)
        {
            _teamRepository = teamRepository;
            _currentUser = currentUser;
        }

        public async Task<CreateTeamResponse> CreateTeamAsync(CreateTeamRequest request)
        {
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                JoinCode = GenerateJoinCode(),
                OwnerId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _teamRepository.AddAsync(team);

            return new CreateTeamResponse(team.Id, team.Name, team.JoinCode);
        }

        public async Task<IReadOnlyCollection<TeamListItemResponse>> GetTeamsAsync()
        {
            var teams = await _teamRepository.GetAllAsync();

            return teams
                .Select(team => new TeamListItemResponse(
                    team.Id,
                    team.Name,
                    team.JoinCode,
                    team.OwnerId,
                    team.CreatedAt))
                .ToList();
        }

        public async Task<TeamMembersResponse> GetTeamMembersAsync(Guid teamId)
        {
            var team = await _teamRepository.GetByIdWithMembersAsync(teamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found");
            }

            var members = team.Members
                .Select(member => new TeamMemberResponse(
                    member.UserId,
                    team.OwnerId == member.UserId ? "Owner" : "Member",
                    member.CreatedAt))
                .ToList();

            if (team.OwnerId.HasValue && members.All(member => member.UserId != team.OwnerId.Value))
            {
                members.Insert(0, new TeamMemberResponse(team.OwnerId.Value, "Owner", team.CreatedAt));
            }

            return new TeamMembersResponse(team.Id, team.Name, team.OwnerId, members);
        }

        public async Task<JoinTeamResponse> JoinTeamAsync(JoinTeamRequest request)
        {
            var team = await _teamRepository.GetByJoinCodeAsync(request.JoinCode);

            if (team == null)
            {
                throw new InvalidOperationException("Invalid join code");
            }

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = _currentUser.UserId,
                Permissions = TeamMemberPermission.None,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _teamRepository.AddMemberAsync(teamMember);

            return new JoinTeamResponse(team.Id, team.Name);
        }

        private string GenerateJoinCode()
        {
         const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; 
         var random = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
        public async Task RemoveMemberAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found");
            }

            if (team.OwnerId != _currentUser.UserId)
            {
                throw new UnauthorizedAccessException("Only the team owner can remove members.");
            }

            if (team.OwnerId == userId)
            {
                throw new InvalidOperationException("Cannot remove the team owner");
            }

            var member = await _teamRepository.GetMembershipAsync(teamId, userId);
            if (member == null)
            {
                throw new InvalidOperationException("Team member not found");
            }

            await _teamRepository.RemoveMemberAsync(member);
            await _teamRepository.SaveChangesAsync();
        }

        public async Task LeaveTeamAsync(Guid teamId)
        {
            var userId = _currentUser.UserId;

            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found");
            }

            if (team.OwnerId == userId)
            {
                throw new InvalidOperationException("Team owner cannot leave the team");
            }

            var member = await _teamRepository.GetMembershipAsync(teamId, userId);
            if (member == null)
            {
                throw new InvalidOperationException("You are not a member of this team");
            }

            await _teamRepository.RemoveMemberAsync(member);
            await _teamRepository.SaveChangesAsync();
        }
    }
}
