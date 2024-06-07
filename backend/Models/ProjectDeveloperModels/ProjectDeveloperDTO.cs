namespace developers.Models
{
    public class ProjectDeveloperDTO
    {
        public int? DeveloperId { get; set; }
        public int? ProjectId { get; set; }
        public string? DeveloperName { get; set; }
        public string? ProjectName { get; set; }
        public string? Accepted { get; set; }
    }
}
