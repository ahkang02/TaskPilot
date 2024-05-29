namespace TaskPilot.Web.ViewModels
{
    public class TaskDetailViewModel
    {
        public Guid? Id { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? DueDate { get; set; }

        public required string AssignFrom { get; set; }

        public string? AssignFromRole { get; set; }

        public required string AssignTo { get; set; }

        public required string Priority { get; set; }

        public required string Status { get; set; }
    }
}
