using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories.Interfaces;

namespace TaskFlow_Pro.Repositories.Implementations
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _context.Teams
                .Include(t => t.Leader)
                .Include(t => t.Members)
                .ToListAsync();
        }

        public async Task<Team?> GetByIdAsync(int ? id)
        {
            return await _context.Teams
                .Include(t => t.Leader)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team?> GetByUserIdAsync(string userId)
        {
            return await _context.Teams
                .Include(t => t.Leader)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Members.Any(u => u.Id == userId));
        }

        public async Task AddAsync(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }
}