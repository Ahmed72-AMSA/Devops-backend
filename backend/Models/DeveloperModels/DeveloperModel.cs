using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace developers.Models
{
    public class Developer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 


        public int ID { get; set; }

        public int UserID { get; set; }

        public User? user { get; set; }

        public ICollection<Project>? Projects { get; set; }
        public List<ProjectDeveloper>? ProjectDevelopers { get; set; }
        public ICollection<TaskCard>? Tasks {get; set;}
        public List<TaskDeveloper>? TaskDevelopers {get; set;}




    }
}
