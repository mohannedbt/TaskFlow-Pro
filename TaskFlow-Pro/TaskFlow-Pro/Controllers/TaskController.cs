using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private const int PageSize = 5;


        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
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
                tasks = await _taskService.GetAllTasksAsync();

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

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchQuery = q;
            ViewBag.State = state;

            return View(pagedTasks);
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
                "anonymous",
                model.StartDate,
                model.EndDate
            );

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