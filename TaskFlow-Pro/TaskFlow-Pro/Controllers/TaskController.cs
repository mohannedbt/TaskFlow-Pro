using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // ====================================
        // TASK BOARD + SEARCH
        // ====================================
        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var tasks = string.IsNullOrWhiteSpace(q)
                ? await _taskService.GetAllTasksAsync()
                : await _taskService.SearchTasksAsync(q);

            ViewBag.SearchQuery = q;

            return View(tasks);
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