using Microsoft.AspNetCore.Authorization;
using MvP.Domain.Enums.Teams;
public class RequireTeamPermissionAttribute : AuthorizeAttribute
{
    public RequireTeamPermissionAttribute(TeamMemberPermission requiredPermission)
    {
        Policy = $"TeamPermission:{requiredPermission}";
    }
}


