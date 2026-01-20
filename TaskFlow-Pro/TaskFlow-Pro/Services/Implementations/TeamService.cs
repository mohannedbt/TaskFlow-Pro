using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow_Pro.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamService(
            ITeamRepository teamRepo,
            UserManager<ApplicationUser> userManager)
        {
            _teamRepo = teamRepo;
            _userManager = userManager;
        }

        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _teamRepo.GetAllAsync();
        }

        public async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await _teamRepo.GetByIdAsync(id);
        }

        public async Task<Team?> GetTeamForUserAsync(string userId)
        {
            return await _teamRepo.GetByUserIdAsync(userId);
        }

        // =========================
        // CREATE TEAM
        // =========================
        public async Task<Team> CreateTeamAsync(string name, string? description, ApplicationUser leader)
        {
            if (leader.TeamId != null)
                throw new InvalidOperationException("User already in a team.");

            var team = new Team
            {
                Name = name,
                Description = description,
                LeaderId = leader.Id
            };

            team.Members.Add(leader);
            leader.Team = team;

            await _teamRepo.AddAsync(team);
            return team;
        }

        // =========================
        // JOIN TEAM
        // =========================
        public async Task JoinTeamAsync(int teamId, ApplicationUser user)
        {
            if (user.TeamId != null)
                throw new InvalidOperationException("User already in a team.");

            var team = await _teamRepo.GetByIdAsync(teamId)
                       ?? throw new InvalidOperationException("Team not found.");

            user.TeamId = team.Id;
            await _teamRepo.UpdateAsync(team);
        }

        // =========================
        // LEAVE TEAM
        // =========================
        public async Task LeaveTeamAsync(ApplicationUser user)
        {
            if (user.TeamId == null)
                return;

            var team = await _teamRepo.GetByIdAsync(user.TeamId.Value)
                       ?? throw new InvalidOperationException("Team not found.");

            if (team.LeaderId == user.Id)
                throw new InvalidOperationException("Leader must transfer leadership before leaving.");

            team.Members.Remove(user);
            user.TeamId = null;

            await _teamRepo.UpdateAsync(team);
        }
        //======================
        // Get Team Member
        //=====================
        public async Task<List<ApplicationUser>> GetTeamMembersAsync(int teamid)
        {
            var team = await _teamRepo.GetByIdAsync(teamid);
            if (team != null)
            {
                List<ApplicationUser> members=team.Members.ToList();
                return members;
            }
            else
            {
                return null;
            }

           
        }

        // =========================
        // REMOVE MEMBER (LEADER ONLY)
        // =========================
        public async Task RemoveMemberAsync(int teamId, string memberId, string leaderId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                       ?? throw new InvalidOperationException("Team not found.");

            if (team.LeaderId != leaderId)
                throw new UnauthorizedAccessException();

            var member = team.Members.FirstOrDefault(u => u.Id == memberId)
                         ?? throw new InvalidOperationException("Member not found.");

            team.Members.Remove(member);
            member.TeamId = null;

            await _teamRepo.UpdateAsync(team);
        }

        // =========================
        // TRANSFER LEADERSHIP
        // =========================
        public async Task TransferLeadershipAsync(int teamId, string newLeaderId, string currentLeaderId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                       ?? throw new InvalidOperationException("Team not found.");

            if (team.LeaderId != currentLeaderId)
                throw new UnauthorizedAccessException();

            if (!team.Members.Any(u => u.Id == newLeaderId))
                throw new InvalidOperationException("New leader must be a member.");

            team.LeaderId = newLeaderId;
            await _teamRepo.UpdateAsync(team);
        }
        // ========================
        // IS team Leader
        // =========================
        public async Task<bool> IsTeamLeaderAsync(int? teamId, string userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId);
            return team.LeaderId == userId;
        }
    }
}
