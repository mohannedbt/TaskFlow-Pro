using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models{
public class WorkspaceViewModel
{
    [Required, StringLength(60)]
    public string WorkspaceName { get; set; } = null!;

    [Range(1, 5000)]
    public int MaxMembers { get; set; } = 50;

    // user types "@x.com"
    [Required, StringLength(120)]
    public string EmailRule { get; set; } = null!;
}
}