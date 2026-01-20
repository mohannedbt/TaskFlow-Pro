using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Pro.Models
{
    public class CreateTeamViewModel
    {
        [Required, StringLength(60)]
        public string Name { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }
    }
}