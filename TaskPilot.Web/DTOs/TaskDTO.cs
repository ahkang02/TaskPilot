namespace TaskPilot.Web.DTOs
{
    public class TaskDTO
    {
        public Guid? Id { get; set; }

        public string TaskName { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public string AssignTo { get; set; }

        public DateTime? dueDate { get; set; }
    }
}
