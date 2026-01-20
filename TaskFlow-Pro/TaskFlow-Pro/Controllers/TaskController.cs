using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ITeamService _teamService;
        private readonly UserManager<ApplicationUser> _userManager;

        private const int PageSize = 5;

        public TaskController(
            ITaskService taskService,
            ITeamService teamService,
            UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _teamService = teamService;
            _userManager = userManager;
        }

        // -------------------------
        // Helpers
        // -------------------------
        private async Task<(ApplicationUser me, int workspaceId)> GetMeAndWorkspaceAsync()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) throw new UnauthorizedAccessException("User not found.");
            if (me.WorkspaceId == null) throw new UnauthorizedAccessException("User has no workspace.");
            return (me, me.WorkspaceId);
        }

        // =========================================================
        // MY CREATED TASKS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MyCreatedTasks(TaskFilterViewModel filters, int page = 1)
        {
            var (me, wsId) = await GetMeAndWorkspaceAsync();

            var tasks = await _taskService.GetAllTasksByCreatorIDAsync(me.Id, wsId);
            tasks = await _taskService.ApplyFiltersAsync(tasks, filters);

            return View("TaskList", await BuildViewModelAsync(tasks, filters, page, wsId));
        }

        // =========================================================
        // MY ASSIGNED TASKS (MODERN: FROM TaskUserProgress)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MyToDo(TaskFilterViewModel filters, int page = 1)
        {
            var (me, wsId) = await GetMeAndWorkspaceAsync();

            var tasks = await _taskService.GetTasksAssignedToUserAsync(me.Id, wsId);
            tasks = await _taskService.ApplyFiltersAsync(tasks, filters);

            return View("TaskList", await BuildViewModelAsync(tasks, filters, page, wsId));
        }

        // =========================================================
        // TEAM TASKS (LEADER VIEW)
        // =========================================================
        [HttpGet]
       

        // =========================================================
        // ASSIGN TASK TO TEAM (CREATOR)
        // =========================================================
        // =========================================================
// ASSIGN TASK TO TEAM (CREATOR)
// =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToTeam(int taskId, int teamId)
        {
            var (me, wsId) = await GetMeAndWorkspaceAsync();

            // Load task inside workspace
            var task = await _taskService.GetAllTasksByTaskIdAsync(taskId, wsId);
            if (task == null) return NotFound();

            // Only creator can assign (your rule)
            if (task.CreatedById != me.Id) return Forbid();

            // Team must belong to same workspace (important)
            var team = await _teamService.GetTeamByIdAsync(teamId, wsId);
            if (team == null) return NotFound("Team not found in your workspace.");

            // Do the assignment in service (recommended)
            await _taskService.AssignTaskToTeamAsync(taskId, teamId, wsId);

            return Redirect(Request.Headers["Referer"].ToString());
        
        }

        // =========================================================
        // MEMBER ACTION: UPDATE MY PROGRESS (TaskUserProgress)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
   
        public async Task<IActionResult> SetMyState(int taskId, State newState)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (me.WorkspaceId == null) return Forbid();

            await _taskService.SetMyProgressStateAsync(taskId, me.Id, me.WorkspaceId, newState);

            return Redirect(Request.Headers["Referer"].ToString());
        }


        // =========================================================
        // GLOBAL TASK STATE (LEADER / CREATOR)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeState(int taskId, State newState)
        {
            var (me, wsId) = await GetMeAndWorkspaceAsync();

            var task = await _taskService.GetAllTasksByTaskIdAsync(taskId, wsId);
            if (task == null) return NotFound();

            bool isLeader = false;
            if (task.TeamId.HasValue)
                isLeader = await _teamService.IsTeamLeaderAsync(task.TeamId.Value, me.Id,wsId);

            if (!TaskPolicy.CanChangeState(task, me.Id, newState, isLeader))
                return Forbid();

            await _taskService.ChangeTaskStateAsync(taskId, newState, me.Id);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // =========================================================
        // CREATE TASK
        // =========================================================
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (me, wsId) = await GetMeAndWorkspaceAsync();

            await _taskService.CreateTaskAsync(
                model.Title,
                model.Description,
                me.Id,
                model.StartDate,
                model.EndDate,
                wsId // ✅ IMPORTANT (fixes FK workspace error)
            );

            return RedirectToAction(nameof(MyCreatedTasks));
        }

        // =========================================================
        // BULK ACTIONS (GLOBAL - keep as is or restrict to leader)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(BulkTaskActionViewModel model)
        {
            var (me, wsId) = await GetMeAndWorkspaceAsync();

            if (model.TaskIds == null || !model.TaskIds.Any())
                return RedirectToAction(nameof(MyCreatedTasks));

            foreach (var taskId in model.TaskIds)
            {
                var task = await _taskService.GetAllTasksByTaskIdAsync(taskId, wsId);
                if (task == null) continue;

                bool isLeader = false;
                if (task.TeamId.HasValue)
                    isLeader = await _teamService.IsTeamLeaderAsync(task.TeamId.Value, me.Id,wsId);

                if (TaskPolicy.CanChangeState(task, me.Id, model.Action, isLeader))
                    await _taskService.ChangeTaskStateAsync(taskId, model.Action, me.Id);
            }

            return RedirectToAction(nameof(MyCreatedTasks));
        }

        // =========================================================
        // VIEW MODEL BUILDER
        // Adds MyState per task for UI
        // =========================================================
        private async Task<TaskViewModel> BuildViewModelAsync(
            List<TaskItem> tasks,
            TaskFilterViewModel filters,
            int page,
            int workspaceId)
        {
            int totalItems = tasks.Count;

            var pagedTasks = tasks
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var (me, _) = await GetMeAndWorkspaceAsync();

            // Prefer a workspace-scoped method if you have it; otherwise keep your existing one.
            // Recommended: _teamService.GetAllTeamsInWorkspaceAsync(workspaceId)
            var teams = await _teamService.GetAllTeamsAsync(workspaceId);

            var myStates = new Dictionary<int, State?>();
            foreach (var t in pagedTasks)
                myStates[t.Id] = await _taskService.GetMyStateAsync(t.Id, me.Id);

            return new TaskViewModel
            {
                TaskItems = pagedTasks,
                Users = _userManager.Users.ToList(),
                Teams = teams,
                Filters = filters,
                MyStates = myStates
            };
        }
    }
}
