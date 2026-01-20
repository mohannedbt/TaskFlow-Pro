using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface ITeamService
    {
        // =========================
        // READ
        // =========================
        Task<List<Team>> GetAllTeamsAsync();
        Task<Team?> GetTeamByIdAsync(int id);
        Task<Team?> GetTeamForUserAsync(string userId);
        Task<List<ApplicationUser>> GetTeamMembersAsync(int teamId);

        // =========================
        // CREATE / JOIN / LEAVE
        // =========================
        Task<Team> CreateTeamAsync(
            string name,
            string? description,
            ApplicationUser leader);

        Task JoinTeamAsync(int teamId, ApplicationUser user);
        Task LeaveTeamAsync(ApplicationUser user);

        // =========================
        // LEADER ACTIONS
        // =========================
        Task RemoveMemberAsync(
            int teamId,
            string memberId,
            string leaderId);

        Task TransferLeadershipAsync(
            int teamId,
            string newLeaderId,
            string currentLeaderId);

        Task<bool> IsTeamLeaderAsync(int? teamId, string userId);
    }
}