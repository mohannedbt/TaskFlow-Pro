namespace TaskFlow_Pro.Models;

public class TaskItem
{
    public int Id {get; set;}
    public DateTime StartDate {get; set;}
    public DateTime EndDate {get; set;}
    public int Duration {get; set;}
    public string Title {get; set;}
    public string Description {get; set;}
    
    public string CreatorById {get; set;}
    public ApplicationUser CreatedBy {get; set;}
    public ApplicationUser AssignedTo {get; set;}
    public State State {get; set;}
    public ICollection<Comment> Comments {get; set;}
}