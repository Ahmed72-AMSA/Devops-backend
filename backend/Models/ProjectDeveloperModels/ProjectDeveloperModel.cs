using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace developers.Models
{
    public class ProjectDeveloper
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public Project? Project { get; set; }

        [Required]
        public int DeveloperID { get; set; }

        [ForeignKey("DeveloperID")]
        public Developer? Developer { get; set; }

        public string Accepted { get; set; } = "Not Yet";

    }
}
