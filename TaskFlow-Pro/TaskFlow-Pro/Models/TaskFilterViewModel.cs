namespace TaskFlow_Pro.Models;

using TaskFlow_Pro.Models;


public class TaskFilterViewModel
{
    public string? Q { get; set; }
    public State? State { get; set; }

    // "today", "week", "custom", or null
    public string? Range { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    // "start_asc", "start_desc", "end_asc", "end_desc"
    public string Sort { get; set; } = "start_desc";

    // Optional handy toggles
    public bool OnlyUnassigned { get; set; } = false;
}
