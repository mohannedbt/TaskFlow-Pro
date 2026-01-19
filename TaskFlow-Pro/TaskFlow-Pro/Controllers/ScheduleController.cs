using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly ITaskService _taskService;

        public ScheduleController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // =========================
        // CALENDAR PAGE
        // =========================
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // EVENTS FOR CALENDAR (JSON)
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var tasks = await _taskService.GetAllTasksAsync();

            var events = tasks.Select(t => new
            {
                id = t.Id,
                title = t.Title,
                start = t.StartDate,
                end = t.EndDate,
                color = t.State switch
                {
                    State.Completed   => "#198754",
                    State.Ongoing     => "#0d6efd",
                    State.Interrupted => "#ffc107",
                    State.Canceled    => "#dc3545",
                    _                 => "#6c757d"
                }
            });

            return Json(events);
        }
    }
}