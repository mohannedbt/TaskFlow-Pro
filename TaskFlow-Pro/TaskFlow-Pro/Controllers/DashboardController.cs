using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Pro.Services.Interfaces;
using TaskFlow_Pro.Services;

namespace TaskFlow_Pro.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        // /Dashboard?workspaceId=1&days=30
        public async Task<IActionResult> Index(int workspaceId, int days = 30, string mode = "db")
        {
            if (days <= 0) days = 30;
            if (days > 365) days = 365;

            var to = DateTime.UtcNow;
            var from = to.AddDays(-days);

            if (mode == "dummy")
            {
                var vmDummy = DummyDashboardFactory.Build(workspaceId, from, to);
                ViewBag.IsDummy = true;
                return View(vmDummy);
            }

            var vm = await _dashboard.GetWorkspaceDashboardAsync(workspaceId, from, to);
            ViewBag.IsDummy = false;
            return View(vm);
        }

    }
}
