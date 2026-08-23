using MvP.Domain.Enums.Teams;
using MvP.Domain.Entities;

namespace MvP.Domain.Entities.Teams
{
    public class TeamMember : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public TeamMemberPermission Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
