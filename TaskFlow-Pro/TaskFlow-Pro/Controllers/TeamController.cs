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

        public TeamController(
            ITeamService teamService,
            UserManager<ApplicationUser> userManager)
        {
            _teamService = teamService;
            _userManager = userManager;
        }

        // =========================
        // LIST ALL TEAMS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return View(teams);
        }

        // =========================
        // MY TEAM
        // =========================
        [HttpGet]
        public async Task<IActionResult> MyTeam()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var team = await _teamService.GetTeamForUserAsync(user.Id);
            return View(team);
        }

        // =========================
        // CREATE TEAM
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                await _teamService.CreateTeamAsync(name, description, user);
                return RedirectToAction(nameof(MyTeam));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================
        // JOIN TEAM
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int teamId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                await _teamService.JoinTeamAsync(teamId, user);
                return RedirectToAction(nameof(MyTeam));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================
        // LEAVE TEAM
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                await _teamService.LeaveTeamAsync(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(MyTeam));
            }
        }

        // =========================
        // REMOVE MEMBER (LEADER)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, string memberId)
        {
            var leader = await _userManager.GetUserAsync(User);
            if (leader == null)
                return Unauthorized();

            await _teamService.RemoveMemberAsync(teamId, memberId, leader.Id);
            return RedirectToAction(nameof(MyTeam));
        }
    }
}