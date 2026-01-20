namespace TaskFlow_Pro.Models
{
    public static class TaskPolicy
    {
        public static bool CanChangeState(
            TaskItem task,
            string userId,
            State newState,
            bool isTeamLeader = false)
        {
            bool isAssignedUser =
                task.AssignedUsers != null &&
                task.AssignedUsers.Any(u => u.Id == userId);

            bool isCreator = task.CreatedById == userId;

            // =========================
            // ASSIGNED USERS CONTROL EXECUTION
            // =========================
            if (isAssignedUser)
            {
                return newState switch
                {
                    State.Interrupted => task.State == State.Ongoing,
                    State.Ongoing     => task.State == State.Interrupted,
                    State.Completed   => task.State == State.Ongoing,
                    _ => false
                };
            }

            // =========================
            // CREATOR RIGHTS
            // =========================
            if (isCreator)
            {
                // Creator can cancel anytime except after completion
                if (newState == State.Canceled && task.State != State.Completed)
                    return true;
            }

            // =========================
            // TEAM LEADER OVERRIDE
            // =========================
            if (isTeamLeader)
            {
                return newState switch
                {
                    State.Canceled    => true,
                    State.Interrupted => task.State == State.Ongoing,
                    State.Ongoing     => task.State == State.Interrupted,
                    _ => false
                };
            }

            return false;
        }
    }
}