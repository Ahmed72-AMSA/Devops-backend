namespace developers.DTOs
{
    public class TaskDeveloperViewModel
    {

        public int ID { get; set;}
        public int TaskId { get; set; }
        public int DeveloperId { get; set; }
        public int ProjectId { get; set; }
        public string? DeveloperName { get; set; }
        public string? TaskName { get; set; }
        public string? ProjectName { get; set; }
        public string? Status { get; set; }

        public string? taskImage {get; set;}
    }
}
