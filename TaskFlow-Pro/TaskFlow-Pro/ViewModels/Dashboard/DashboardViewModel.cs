namespace TaskFlow_Pro.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int WorkspaceId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public DashboardKpis Kpis { get; set; } = new();
        public List<DailyCompletedPoint> DailyCompleted { get; set; } = new();
        public List<TeamVelocityRow> TeamVelocity { get; set; } = new();
        public List<MemberLoadRow> MemberLoad { get; set; } = new();
    }

    public class DashboardKpis
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double CompletionRatePct { get; set; }
        public double AvgCompletionHours { get; set; }
        public int ActiveTasks { get; set; }
    }

    public class DailyCompletedPoint
    {
        public DateTime Day { get; set; }
        public int Count { get; set; }
    }

    public class TeamVelocityRow
    {
        public int? TeamId { get; set; }
        public string TeamName { get; set; } = "No Team";
        public int CompletedTasks { get; set; }
        public double AvgCompletionHours { get; set; }
    }

    public class MemberLoadRow
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public int ActiveAssignments { get; set; }     // ongoing progresses
        public int CompletedAssignments { get; set; }  // completed progresses
    }
}
