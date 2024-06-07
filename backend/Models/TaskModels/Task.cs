using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace developers.Models
{
    public class TaskCard
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; set; }
        public required int ProjectID { get; set; } // Add this line

        public Project? Project { get; set; } // Add this line

        public ICollection<Developer>? Developers { get; set; }
        public List<TaskDeveloper>? TaskDevelopers { get; set; }


        [NotMapped]
        public IFormFile? ImageFile { get; set; } 

        [AllowNull]
        public string? ImageFilePath { get; set; } 

        public ICollection<Comments>? Comments {get; set;}




        
    }
}
