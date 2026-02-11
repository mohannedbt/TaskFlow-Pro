using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models; // <-- adapte si ton DbContext est ailleurs
using TaskFlow_Pro.Services.Interfaces;
using TaskFlow_Pro.ViewModels.Dashboard;

namespace TaskFlow_Pro.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;

        public DashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardViewModel> GetWorkspaceDashboardAsync(int workspaceId, DateTime from, DateTime to)
        {
            // Normaliser plage
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1).AddTicks(-1);

            // Base query Tasks in workspace & period (StartDate as period filter)
            var tasksQ = _db.Tasks
                .AsNoTracking().ToList() ;
            var totalTasks = tasksQ.Count();
            var completedTasks = tasksQ.Count(t => t.State == State.Completed);
            var activeTasks = tasksQ.Count(t => t.State != State.Completed);

            // Avg completion time based on user progress completedAt - task.StartDate
            var avgCompletionHours = await _db.TaskUserProgresses
                .AsNoTracking()
                .Where(p => p.WorkspaceId == workspaceId
                            && p.State == State.Completed
                            && p.CompletedAt != null
                            && p.CompletedAt >= fromDate
                            && p.CompletedAt <= toDate)
                .Select(p => EF.Functions.DateDiffMinute(p.TaskItem.StartDate, p.CompletedAt!.Value))
                .AverageAsync(x => (double?)x) ?? 0.0;

            avgCompletionHours = avgCompletionHours / 60.0;

            // Daily completed tasks (by TaskItem.State == Completed, grouped by EndDate day)
            var daily = await _db.Tasks
                .Where(t => t.State == State.Completed)
                .GroupBy(t => t.EndDate.Date)
                .Select(g => new DailyCompletedPoint { Day = g.Key, Count = g.Count() })
                .OrderBy(x => x.Day)
                .ToListAsync();

            // Team velocity (completed tasks per team)
            var teamVelocity = await _db.Tasks
                .Where(t => t.State == State.Completed)
                .GroupBy(t => new { t.TeamId, TeamName = t.Team != null ? t.Team.Name : "No Team" })
                .Select(g => new TeamVelocityRow
                {
                    TeamId = g.Key.TeamId,
                    TeamName = g.Key.TeamName,
                    CompletedTasks = g.Count(),
                    AvgCompletionHours = g.Average(t => EF.Functions.DateDiffMinute(t.StartDate, t.EndDate)) / 60.0
                })
                .OrderByDescending(x => x.CompletedTasks)
                .ToListAsync();

            // Member load (progress ongoing vs completed) in workspace + period overlap
            var memberLoad = await _db.TaskUserProgresses
                .AsNoTracking()
                .Where(p => p.WorkspaceId == workspaceId
                            && p.TaskItem.StartDate <= toDate
                            && p.TaskItem.EndDate >= fromDate)
                .GroupBy(p => new { p.UserId, UserName = p.User.UserName })
                .Select(g => new MemberLoadRow
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName ?? g.Key.UserId,
                    ActiveAssignments = g.Count(p => p.State != State.Completed),
                    CompletedAssignments = g.Count(p => p.State == State.Completed)
                })
                .OrderByDescending(x => x.ActiveAssignments)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                WorkspaceId = workspaceId,
                From = fromDate,
                To = toDate,
                Kpis = new DashboardKpis
                {
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    ActiveTasks = activeTasks,
                    CompletionRatePct = totalTasks == 0 ? 0 : Math.Round(100.0 * completedTasks / totalTasks, 2),
                    AvgCompletionHours = Math.Round(avgCompletionHours, 2)
                },
                DailyCompleted = daily,
                TeamVelocity = teamVelocity,
                MemberLoad = memberLoad
            };

            return vm;
        }
    }
}
