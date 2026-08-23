namespace MvP.Application.DTOs;
public record CreateTeamRequest(string Name, Guid OwnerId);
public record CreateTeamResponse(Guid TeamId, string Name, string JoinCode);

public record JoinTeamRequest(string JoinCode,Guid UserId);
public record JoinTeamResponse(Guid TeamId, string Name);
public record TeamMemberResponse(Guid UserId, string Role, DateTime JoinedAt);