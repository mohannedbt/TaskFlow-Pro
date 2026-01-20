using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface ITeamService
    {
        Task<List<Team>> GetAllTeamsAsync(int workspaceId);
        Task<Team?> GetTeamByIdAsync(int teamId, int workspaceId);
        Task<Team?> GetTeamForUserAsync(string userId);
        Task<List<ApplicationUser>> GetTeamMembersAsync(int teamId, int workspaceId);

        Task<Team> CreateTeamAsync(string name, string? description, ApplicationUser leader);
        Task JoinTeamAsync(int teamId, ApplicationUser user);
        Task LeaveTeamAsync(ApplicationUser user);

        Task RemoveMemberAsync(int teamId, string memberId, string leaderId, int workspaceId);
        Task TransferLeadershipAsync(int teamId, string newLeaderId, string currentLeaderId, int workspaceId);
        Task<bool> IsTeamLeaderAsync(int teamId, string userId, int workspaceId);
    }
}