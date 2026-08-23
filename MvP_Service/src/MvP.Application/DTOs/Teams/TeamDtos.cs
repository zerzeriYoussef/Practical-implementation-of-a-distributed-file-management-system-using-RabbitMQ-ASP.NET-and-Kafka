namespace MvP.Application.DTOs;
public record CreateTeamRequest(string Name);
public record CreateTeamResponse(Guid TeamId, string Name, string JoinCode);

public record JoinTeamRequest(string JoinCode);
public record JoinTeamResponse(Guid TeamId, string Name);
public record TeamListItemResponse(Guid TeamId, string Name, string JoinCode, Guid? OwnerId, DateTime CreatedAt);
public record TeamMemberResponse(Guid UserId, string Role, DateTime JoinedAt);
public record TeamMembersResponse(Guid TeamId, string Name, Guid? OwnerId, IReadOnlyCollection<TeamMemberResponse> Members);
