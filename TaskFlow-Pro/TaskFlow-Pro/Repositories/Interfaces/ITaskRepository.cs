using TaskFlow_Pro.Models;

namespace TaskFlow_Pro.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        // =====================================================
        // BASIC QUERIES (foundational LINQ)
        // =====================================================

        /// <summary>
        /// Get a task by its ID (with related data).
        /// </summary>
        Task<TaskItem?> GetByIdAsync(int taskId);

        /// <summary>
        /// Get all tasks.
        /// </summary>
        Task<List<TaskItem>> GetAllAsync();

        /// <summary>
        /// Get tasks assigned to a given user.
        /// </summary>
        Task<List<TaskItem>> GetTasksAssignedToUserAsync(string userId);

        /// <summary>
        /// Get tasks filtered by state.
        /// </summary>
        Task<List<TaskItem>> GetTasksByStateAsync(State state);

        // =====================================================
        // ADVANCED QUERIES (REAL LINQ SKILLS)
        // =====================================================


        /// <summary>
        /// Get tasks waiting for review.
        /// </summary>
        Task<List<TaskItem>> GetTasksWaitingForReviewAsync();

        /// <summary>
        /// Get tasks created in the last X days.
        /// </summary>
        Task<List<TaskItem>> GetRecentTasksAsync(int days);

        /// <summary>
        /// Get tasks grouped by state.
        /// </summary>
        Task<Dictionary<State, int>> GetTaskCountByStateAsync();

        /// <summary>
        /// Get tasks ordered by creation date (newest first).
        /// </summary>
        Task<List<TaskItem>> GetTasksOrderedByDateAsync();

        /// <summary>
        /// Search tasks by keyword in title or description.
        /// </summary>
        Task<List<TaskItem>> SearchTasksAsync(string keyword);

        // =====================================================
        // WRITE OPERATIONS (ASYNC DB MODIFICATIONS)
        // =====================================================

        /// <summary>
        /// Add a new task.
        /// </summary>
        Task AddAsync(TaskItem task);

        /// <summary>
        /// Update an existing task.
        /// </summary>
        Task UpdateAsync(TaskItem task);

        public Task<List<TaskItem>> GetAllByCreator(string id);

    }
}
