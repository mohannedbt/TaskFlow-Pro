using Microsoft.AspNetCore.Identity;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamService(ITeamRepository teamRepo, UserManager<ApplicationUser> userManager)
        {
            _teamRepo = teamRepo;
            _userManager = userManager;
        }

        // =========================
        // READ (workspace scoped)
        // =========================
        public Task<List<Team>> GetAllTeamsAsync(int workspaceId)
            => _teamRepo.GetAllByWorkspaceAsync(workspaceId);

        public Task<Team?> GetTeamByIdAsync(int teamId, int workspaceId)
            => _teamRepo.GetByIdInWorkspaceAsync(teamId, workspaceId);

        public Task<Team?> GetTeamForUserAsync(string userId)
            => _teamRepo.GetByUserIdAsync(userId);

        public async Task<List<ApplicationUser>> GetTeamMembersAsync(int teamId, int workspaceId)
        {
            var team = await _teamRepo.GetByIdInWorkspaceAsync(teamId, workspaceId);
            if (team == null) return new List<ApplicationUser>();
            return team.Members?.ToList() ?? new List<ApplicationUser>();
        }

        // =========================
        // CREATE TEAM
        // =========================
        public async Task<Team> CreateTeamAsync(string name, string? description, ApplicationUser leader)
        {
            if (leader.WorkspaceId == null)
                throw new InvalidOperationException("User has no workspace.");

            if (leader.TeamId != null)
                throw new InvalidOperationException("User already in a team.");

            name = (name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Team name is required.");

            // optional: prevent duplicate team names in same workspace
            var exists = await _teamRepo.GetAllByWorkspaceAsync(leader.WorkspaceId);
            var names=from n in exists where n.Name==name select n.Name;
            if (names.Count() > 0)
            {


                throw new InvalidOperationException("A team with that name already exists.");
            }

            var team = new Team
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                LeaderId = leader.Id,
                WorkspaceId = leader.WorkspaceId
            };

            // attach leader to team
            team.Members.Add(leader);
            leader.TeamId = team.Id; // will be set after save; we’ll re-update after add
            leader.Team = team;

            await _teamRepo.AddAsync(team);

            // After team is saved, ensure leader.TeamId persisted
            leader.TeamId = team.Id;
            var res = await _userManager.UpdateAsync(leader);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));

            return team;
        }

        // =========================
        // JOIN TEAM
        // =========================
        public async Task JoinTeamAsync(int teamId, ApplicationUser user)
        {
            if (user.WorkspaceId == null)
                throw new InvalidOperationException("User has no workspace.");

            if (user.TeamId != null)
                throw new InvalidOperationException("User already in a team.");

            var team = await _teamRepo.GetByIdInWorkspaceAsync(teamId, user.WorkspaceId)
                       ?? throw new InvalidOperationException("Team not found in your workspace.");

            // add membership
            if (!team.Members.Any(m => m.Id == user.Id))
                team.Members.Add(user);

            user.TeamId = team.Id;

            await _teamRepo.UpdateAsync(team);

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));
        }

        // =========================
        // LEAVE TEAM
        // =========================
        public async Task LeaveTeamAsync(ApplicationUser user)
        {
            if (user.WorkspaceId == null)
                throw new InvalidOperationException("User has no workspace.");

            if (user.TeamId == null)
                return;

            var team = await _teamRepo.GetByIdInWorkspaceAsync(user.TeamId.Value, user.WorkspaceId)
                       ?? throw new InvalidOperationException("Team not found.");

            if (team.LeaderId == user.Id)
                throw new InvalidOperationException("Leader must transfer leadership before leaving.");

            var member = team.Members.FirstOrDefault(u => u.Id == user.Id);
            if (member != null) team.Members.Remove(member);

            user.TeamId = null;

            await _teamRepo.UpdateAsync(team);

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));
        }

        // =========================
        // REMOVE MEMBER (LEADER ONLY)
        // =========================
        public async Task RemoveMemberAsync(int teamId, string memberId, string leaderId, int workspaceId)
        {
            var team = await _teamRepo.GetByIdInWorkspaceAsync(teamId, workspaceId)
                       ?? throw new InvalidOperationException("Team not found.");

            if (team.LeaderId != leaderId)
                throw new UnauthorizedAccessException();

            var member = team.Members.FirstOrDefault(u => u.Id == memberId)
                         ?? throw new InvalidOperationException("Member not found.");

            team.Members.Remove(member);
            member.TeamId = null;

            await _teamRepo.UpdateAsync(team);

            var res = await _userManager.UpdateAsync(member);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));
        }

        // =========================
        // TRANSFER LEADERSHIP
        // =========================
        public async Task TransferLeadershipAsync(int teamId, string newLeaderId, string currentLeaderId, int workspaceId)
        {
            var team = await _teamRepo.GetByIdInWorkspaceAsync(teamId, workspaceId)
                       ?? throw new InvalidOperationException("Team not found.");

            if (team.LeaderId != currentLeaderId)
                throw new UnauthorizedAccessException();

            if (!team.Members.Any(u => u.Id == newLeaderId))
                throw new InvalidOperationException("New leader must be a member.");

            team.LeaderId = newLeaderId;
            await _teamRepo.UpdateAsync(team);
        }

        // =========================
        // IS TEAM LEADER (workspace safe)
        // =========================
        public async Task<bool> IsTeamLeaderAsync(int teamId, string userId, int workspaceId)
        {
            var team = await _teamRepo.GetByIdInWorkspaceAsync(teamId, workspaceId);
            return team != null && team.LeaderId == userId;
        }
    }
}
