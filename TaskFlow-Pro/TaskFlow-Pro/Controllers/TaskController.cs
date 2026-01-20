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

        // =========================================================
        // MY CREATED TASKS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MyTasks(TaskFilterViewModel filters, int page = 1)
        {
            var userId = _userManager.GetUserId(User)!;
            var tasks = await _taskService.GetAllTasksByCreatorIDAsync(userId);

            tasks = await _taskService.ApplyFiltersAsync(tasks, filters);

            return View("TaskList", await BuildViewModelAsync(tasks, filters, page));
        }

        // =========================================================
        // MY ASSIGNED TASKS (MODERN: FROM TaskUserProgress)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MyAssigned(TaskFilterViewModel filters, int page = 1)
        {
            var userId = _userManager.GetUserId(User)!;

            // ✅ Modern: based on TaskUserProgress, not AssignedToId
            var tasks = await _taskService.GetTasksAssignedToUserAsync(userId);

            tasks = await _taskService.ApplyFiltersAsync(tasks, filters);

            return View("TaskList", await BuildViewModelAsync(tasks, filters, page));
        }

        // =========================================================
        // TEAM TASKS (LEADER VIEW)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> TeamTasks(TaskFilterViewModel filters, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeamId == null)
                return Forbid();

            bool isLeader = await _teamService.IsTeamLeaderAsync(user.TeamId.Value, user.Id);
            if (!isLeader)
                return Forbid();

            var tasks = await _taskService.GetAllTasksByTeamIdAsync(user.TeamId.Value);

            tasks = await _taskService.ApplyFiltersAsync(tasks, filters);

            return View("TaskList", await BuildViewModelAsync(tasks, filters, page));
        }

        // =========================================================
        // ASSIGN TASK TO TEAM (CREATOR)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToTeam(int taskId, int teamId)
        {
            var userId = _userManager.GetUserId(User)!;

            var task = await _taskService.GetAllTasksByTaskIdAsync(taskId);
            if (task == null) return NotFound();

            // Optional: only creator can assign
            if (task.CreatedById != userId) return Forbid();

            await _taskService.AssignTaskToTeamAsync(taskId, teamId);

            // Optional: after assigning, you can set task to Ongoing if it was NotAssigned
            // await _taskService.RecomputeTaskStateAsync(taskId);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // =========================================================
        // MEMBER ACTION: UPDATE MY PROGRESS (TaskUserProgress)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMyState(int taskId, State newState)
        {
            var userId = _userManager.GetUserId(User)!;

            var task = await _taskService.GetAllTasksByTaskIdAsync(taskId);
            if (task == null) return NotFound();

            // Basic rule: must be team task to have progress
            if (!task.TeamId.HasValue) return Forbid();

            // You can keep TaskPolicy, but it currently checks global state.
            // For now, enforce minimal safety:
            // - No "Canceled" by members
            if (newState == State.Canceled) return Forbid();

            await _taskService.SetMyProgressStateAsync(taskId, userId, newState);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // =========================================================
        // LEADER / CREATOR GLOBAL ACTIONS (OPTIONAL)
        // Keep ChangeState for global cancels or admin controls
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeState(int taskId, State newState)
        {
            var userId = _userManager.GetUserId(User)!;
            var task = await _taskService.GetAllTasksByTaskIdAsync(taskId);

            if (task == null)
                return NotFound();

            bool isLeader = false;
            if (task.TeamId.HasValue)
            {
                isLeader = await _teamService.IsTeamLeaderAsync(task.TeamId.Value, userId);
            }

            if (!TaskPolicy.CanChangeState(task, userId, newState, isLeader))
                return Forbid();

            await _taskService.ChangeTaskStateAsync(taskId, newState, userId);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // =========================================================
        // CREATE TASK
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User)!;

            await _taskService.CreateTaskAsync(
                model.Title,
                model.Description,
                userId,
                model.StartDate,
                model.EndDate
            );

            return RedirectToAction(nameof(MyTasks));
        }

        // =========================================================
        // BULK ACTIONS (GLOBAL - keep as is or restrict to leader)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(BulkTaskActionViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;

            if (model.TaskIds == null || !model.TaskIds.Any())
                return RedirectToAction(nameof(MyTasks));

            foreach (var taskId in model.TaskIds)
            {
                var task = await _taskService.GetAllTasksByTaskIdAsync(taskId);
                if (task == null) continue;

                bool isLeader = false;
                if (task.TeamId.HasValue)
                {
                    isLeader = await _teamService.IsTeamLeaderAsync(task.TeamId.Value, userId);
                }

                if (TaskPolicy.CanChangeState(task, userId, model.Action, isLeader))
                {
                    await _taskService.ChangeTaskStateAsync(taskId, model.Action, userId);
                }
            }

            return RedirectToAction(nameof(MyTasks));
        }

        // =========================================================
        // VIEW MODEL BUILDER
        // Adds MyState per task for UI
        // =========================================================
        private async Task<TaskViewModel> BuildViewModelAsync(List<TaskItem> tasks, TaskFilterViewModel filters, int page)
        {
            int totalItems = tasks.Count;

            var pagedTasks = tasks
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var userId = _userManager.GetUserId(User)!;
            var teams = await _teamService.GetAllTeamsAsync();

            // ✅ Load my per-task state for UI (simple, one-by-one)
            // Later you can optimize with a bulk query.
            var myStates = new Dictionary<int, State?>();
            foreach (var t in pagedTasks)
            {
                myStates[t.Id] = await _taskService.GetMyStateAsync(t.Id, userId);
            }

            return new TaskViewModel
            {
                TaskItems = pagedTasks,
                Users = _userManager.Users.ToList(),
                Teams = teams,
                Filters = filters,
                MyStates = myStates // ✅ add this property to TaskViewModel
            };
        }
    }
}
