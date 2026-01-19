using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models;

public class TaskViewModel
{
    [Required]
    public ICollection<TaskItem> taskItem { get; set; }
    public ICollection<ApplicationUser>? users { get; set; }
    
}