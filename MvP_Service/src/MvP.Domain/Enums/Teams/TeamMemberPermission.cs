namespace MvP.Domain.Enums.Teams;

[Flags]
public enum TeamMemberPermission
{
    None = 0,
    View = 1,
    Edit = 2,
    Delete = 4,
    Upload = 8,
}
