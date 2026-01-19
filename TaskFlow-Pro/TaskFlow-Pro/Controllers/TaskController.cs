using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private const int PageSize = 5;
        private readonly UserManager<ApplicationUser> _userManager;


        public TaskController(ITaskService taskService, UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _userManager = userManager;
        }

        // ====================================
        // TASK BOARD + SEARCH
        // ====================================
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index(
            string? q,
            string? range,
            State? state,
            DateTime? from,
            DateTime? to,
            bool orderByDate = false,
            int page = 1)
        {
            List<TaskItem> tasks;

            // Date presets
            if (range == "today")
                tasks = await _taskService.GetTasksForTodayAsync();
            else if (range == "week")
                tasks = await _taskService.GetTasksForThisWeekAsync();
            // Custom range
            else if (from.HasValue && to.HasValue)
                tasks = await _taskService.GetTasksByDateRangeAsync(from.Value, to.Value);
            // State filter
            else if (state.HasValue)
                tasks = await _taskService.GetTasksByStateAsync(state.Value);
            // Ordering
            else if (orderByDate)
                tasks = await _taskService.GetTasksOrderedByDateAsync();
            else
                tasks = await _taskService.GetAllTasksByIDAsync(_userManager.GetUserId(User));

            // Search always applied last
            if (!string.IsNullOrWhiteSpace(q))
                tasks = tasks
                    .Where(t => t.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                                || t.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // Pagination
            int totalItems = tasks.Count;
            var pagedTasks = tasks
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            TaskViewModel viewModel = new TaskViewModel();
            viewModel.taskItem=pagedTasks;
            viewModel.users =_userManager.Users.ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchQuery = q;
            ViewBag.State = state;

            return View(viewModel);
        }
// ====================================
// ASSIGN + START TASK
// ====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAndStart(int taskId, string assignedToId)
        {
            // 🔎 Load task
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null)
                return NotFound();

            // 🔎 Load user
            var user = await _userManager.FindByIdAsync(assignedToId);
            if (user == null)
                return NotFound();

            // ✅ Assign user
            task.AssignedTo = user;
            task.State = State.Ongoing;

            // ✅ Persist changes
             await _taskService.UpdateTaskAsync(task);

            return RedirectToAction(nameof(Index));
        }

        // ====================================
        // CREATE TASK
        // ====================================
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

            await _taskService.CreateTaskAsync(
                model.Title,
                model.Description,
                _userManager.GetUserId(User),
                model.StartDate,
                model.EndDate
            ).ConfigureAwait(false);

            return RedirectToAction(nameof(Index));
        }

        // ====================================
        // CHANGE STATE
        // ====================================
        [HttpPost]
        public async Task<IActionResult> ChangeState(int id, State newState)
        {
            await _taskService.ChangeTaskStateAsync(id, newState, "anonymous");
            return RedirectToAction(nameof(Index));
        }
    }
}