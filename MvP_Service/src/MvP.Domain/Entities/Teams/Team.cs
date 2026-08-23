using MvP.Domain.Entities;

namespace MvP.Domain.Entities.Teams
{
    public class Team : BaseEntity
    {   
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public Guid? OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    }
}
