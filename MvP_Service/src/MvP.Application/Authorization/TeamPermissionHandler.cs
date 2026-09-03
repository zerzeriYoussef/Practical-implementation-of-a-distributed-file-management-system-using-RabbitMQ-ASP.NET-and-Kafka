using Microsoft.AspNetCore.Authorization;
using MvP.Domain.Enums.Teams;

public class TeamPermissionHandler
    : AuthorizationHandler<TeamPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TeamPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        // TODO: get user id from token claims
        var userId = context.User.FindFirst("sub")?.Value;

        if (userId == null)
            return;

        // TODO: get teamId from route
        // TODO: check database:
        // Does this user have requirement.Permission in this team?

        var userPermission = TeamMemberPermission.View | TeamMemberPermission.Edit;

        var hasPermission =
            (userPermission & requirement.Permission) == requirement.Permission;

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}