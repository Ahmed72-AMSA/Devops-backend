using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace developers.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public  int ID { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string PasswordSalt { get; set; }

        public string? Type { get; set; } = "developer";

        [Required]
        public required string Name { get; set; }

        public string Role {get; set;} = "User";
        
        public  Developer? Developer { get; set; }
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime? TokenCreated { get; set; }
        public DateTime? TokenExpires { get; set; }
        public ICollection<Comments> Comments {get; set;}



    }
}
