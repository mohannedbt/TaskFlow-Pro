namespace TaskFlow_Pro.Models;

public class BulkTaskActionViewModel
{
    public List<int> TaskIds { get; set; } = new();
    public State Action { get; set; }
}