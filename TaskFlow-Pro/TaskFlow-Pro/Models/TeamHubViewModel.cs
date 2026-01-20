namespace TaskFlow_Pro.Models
{
    public class TeamHubViewModel
    {
        public int WorkspaceId { get; set; }

        public int? MyTeamId { get; set; }
        public string? MyTeamName { get; set; }
        public string? MyTeamDescription { get; set; }
        public string? MyTeamLeaderName { get; set; }

        public List<TeamCardVm> Teams { get; set; } = new();
    }

    public class TeamCardVm
    {
        public int TeamId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int MembersCount { get; set; }
        public bool IsMine { get; set; }
    }
}