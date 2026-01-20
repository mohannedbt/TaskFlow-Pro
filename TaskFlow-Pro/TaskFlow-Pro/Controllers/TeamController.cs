using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamController(ITeamService teamService, UserManager<ApplicationUser> userManager)
        {
            _teamService = teamService;
            _userManager = userManager;
        }

        private bool CanManageTeams()
            => User.IsInRole("Owner") || User.IsInRole("Admin");

        [HttpGet]
        public IActionResult Create()
        {
            if (!CanManageTeams()) return Forbid();
            return View(new CreateTeamViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamViewModel vm)
        {
            if (!CanManageTeams()) return Forbid();

            if (!ModelState.IsValid)
                return View(vm);

            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (me.WorkspaceId == null) return Forbid();

            try
            {
                // NOTE: your current CreateTeamAsync forces leader to not be in a team
                // and auto-joins him. That's ok for now; you'll patch service later
                // to allow admin create without joining.
                await _teamService.CreateTeamAsync(vm.Name.Trim(), vm.Description?.Trim(), me);

                TempData["Success"] = "Team created successfully.";
                return RedirectToAction("Index", "Workspace");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(vm);
            }
        }
         private async Task<(ApplicationUser me, int wsId)> GetMeWs()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) throw new UnauthorizedAccessException();
            if (me.WorkspaceId == null) throw new UnauthorizedAccessException("No workspace");
            return (me, me.WorkspaceId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (me, wsId) = await GetMeWs();

            var teams = await _teamService.GetAllTeamsAsync(wsId);

            var myTeam = me.TeamId.HasValue
                ? teams.FirstOrDefault(t => t.Id == me.TeamId.Value)
                : null;

            var leaderName = myTeam?.LeaderId != null
                ? (await _userManager.FindByIdAsync(myTeam.LeaderId))?.UserName
                : null;

            var vm = new TeamHubViewModel
            {
                WorkspaceId = wsId,
                MyTeamId = myTeam?.Id,
                MyTeamName = myTeam?.Name,
                MyTeamDescription = myTeam?.Description,
                MyTeamLeaderName = leaderName,
                Teams = teams.Select(t => new TeamCardVm
                {
                    TeamId = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    MembersCount = t.Members?.Count ?? 0,
                    IsMine = (me.TeamId == t.Id)
                }).OrderBy(t => t.Name).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int teamId)
        {
            var (me, wsId) = await GetMeWs();

            try
            {
                await _teamService.JoinTeamAsync(teamId, me);
                TempData["Success"] = "Joined team successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave()
        {
            var (me, wsId) = await GetMeWs();

            try
            {
                await _teamService.LeaveTeamAsync(me);
                TempData["Success"] = "You left the team.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
    
}