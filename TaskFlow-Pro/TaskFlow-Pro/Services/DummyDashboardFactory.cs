using TaskFlow_Pro.ViewModels.Dashboard;

namespace TaskFlow_Pro.Services
{
    public static class DummyDashboardFactory
    {
        public static DashboardViewModel Build(int workspaceId, DateTime from, DateTime to)
        {
            var rng = new Random(42);

            var days = Enumerable.Range(0, 14)
                .Select(i => from.Date.AddDays(i))
                .ToList();

            var daily = days.Select(d => new DailyCompletedPoint
            {
                Day = d,
                Count = rng.Next(0, 6)
            }).ToList();

            var total = 48;
            var completed = daily.Sum(x => x.Count);
            if (completed > total) completed = total;

            var teamVelocity = new List<TeamVelocityRow>
            {
                new() { TeamId = 1, TeamName = "Alpha", CompletedTasks = 18, AvgCompletionHours = 11.4 },
                new() { TeamId = 2, TeamName = "Beta", CompletedTasks = 12, AvgCompletionHours = 16.2 },
                new() { TeamId = 3, TeamName = "Gamma", CompletedTasks = 7,  AvgCompletionHours = 22.8 },
            };

            var memberLoad = new List<MemberLoadRow>
            {
                new() { UserId = "u1", UserName = "Hafouz", ActiveAssignments = 5, CompletedAssignments = 9 },
                new() { UserId = "u2", UserName = "Amine",  ActiveAssignments = 7, CompletedAssignments = 4 },
                new() { UserId = "u3", UserName = "Sara",   ActiveAssignments = 2, CompletedAssignments = 10 },
                new() { UserId = "u4", UserName = "Yassine",ActiveAssignments = 4, CompletedAssignments = 6 },
            };

            return new DashboardViewModel
            {
                WorkspaceId = workspaceId,
                From = from.Date,
                To = to.Date,
                Kpis = new DashboardKpis
                {
                    TotalTasks = total,
                    CompletedTasks = completed,
                    ActiveTasks = total - completed,
                    CompletionRatePct = Math.Round(100.0 * completed / total, 2),
                    AvgCompletionHours = 14.7
                },
                DailyCompleted = daily,
                TeamVelocity = teamVelocity,
                MemberLoad = memberLoad
            };
        }
    }
}
