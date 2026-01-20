using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Interfaces;

namespace TaskFlow_Pro.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleController(
            ITaskService taskService,
            UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _userManager = userManager;
        }

        // =========================
        // CALENDAR PAGE
        // =========================
        [HttpGet]
        public IActionResult Index() => View();

        // =========================
        // EVENTS FOR CALENDAR (JSON)
        // Workspace-scoped
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (me.WorkspaceId == null) return Forbid();

            // Option A (recommended): add a service method
            // var tasks = await _taskService.GetAllTasksInWorkspaceAsync(me.WorkspaceId.Value);

            // Option B (quick fix): reuse existing method then filter (less ideal)
            var tasks = await _taskService.GetAllTasks(me.WorkspaceId);

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
