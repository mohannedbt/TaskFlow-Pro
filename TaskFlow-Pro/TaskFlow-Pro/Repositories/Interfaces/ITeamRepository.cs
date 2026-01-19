using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Repositories.Interfaces
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(int id);
        Task<Team?> GetByUserIdAsync(string userId);

        Task AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(Team team);
    }
}