using TaskFlow_Pro.ViewModels.Dashboard;

namespace TaskFlow_Pro.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetWorkspaceDashboardAsync(int workspaceId, DateTime from, DateTime to);
    }
}
