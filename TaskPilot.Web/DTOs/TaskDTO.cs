namespace TaskPilot.Web.DTOs
{
    public class TaskDTO
    {
        public Guid? Id { get; set; }

        public required string TaskName { get; set; }

        public required string Priority { get; set; }

        public required string Status { get; set; }

        public required string AssignTo { get; set; }

        public required string priorityColorCode { get; set; }

        public required string statusColorCode { get; set; }

        public DateTime? dueDate { get; set; }
    }
}
