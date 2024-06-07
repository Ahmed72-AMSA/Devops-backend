using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace developers.Models
{



public class Project
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 

    public int ID { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    public ICollection<Developer>? Developers { get; set; }
    public List<ProjectDeveloper>? ProjectDevelopers { get; set; }

    public ICollection<TaskCard>? Tasks { get; set; } 


    
}

}