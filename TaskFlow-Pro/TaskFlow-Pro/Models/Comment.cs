
namespace TaskFlow_Pro.Models;

public class Comment
{
    public int Id {get; set;}
    public string Content {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}
    public ApplicationUser CreatedBy {get; set;}
    public TaskItem ToTaskItem {get; set;}
    
}