using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace developers.Models
{
    public class TaskDeveloper
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ID { get; set; }

        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")] 
        public TaskCard? Task { get; set; }

        [Required]
        public int DeveloperId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public required string Status { get; set; }


        [ForeignKey("DeveloperId")]
        public Developer? Developer { get; set; }


        public string? FilePath { get; set; } 

        [NotMapped] 
        public IFormFile? File { get; set; } 



    }
}
