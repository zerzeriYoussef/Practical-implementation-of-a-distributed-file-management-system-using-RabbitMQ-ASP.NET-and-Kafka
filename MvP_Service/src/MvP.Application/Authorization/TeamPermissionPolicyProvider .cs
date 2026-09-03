using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MvP.Domain.Enums.Teams;

public class TeamPermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public TeamPermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("TeamPermission:"))
        {
            var permissionName = policyName["TeamPermission:".Length..];

            if (Enum.TryParse<TeamMemberPermission>(permissionName, out var permission))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new TeamPermissionRequirement(permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        return base.GetPolicyAsync(policyName);
    }
}