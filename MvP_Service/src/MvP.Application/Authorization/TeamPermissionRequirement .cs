using Microsoft.AspNetCore.Authorization;
using MvP.Domain.Enums.Teams;

public class TeamPermissionRequirement : IAuthorizationRequirement
{
    public TeamMemberPermission Permission  { get; }

    public TeamPermissionRequirement(TeamMemberPermission Permission )
    {
        this.Permission  = Permission ;
    }
}
