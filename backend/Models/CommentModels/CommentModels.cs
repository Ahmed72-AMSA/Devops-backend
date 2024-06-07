using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace developers.Models
{
    public class Comments
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }


        [Required(ErrorMessage = "Body is required")]
        public string commentBody { get; set; }


        public int TaskId { get; set; }

        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string userEmail {get ; set;}

        public User? Users{get;set;}
        public TaskCard? Task { get; set; }
























        
    }
}
