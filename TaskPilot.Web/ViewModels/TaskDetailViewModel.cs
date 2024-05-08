namespace TaskPilot.Web.ViewModels
{
    public class TaskDetailViewModel
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? DueDate { get; set; }

        public string AssignFrom { get; set; }

        public string AssignFromRole { get; set; }

        public string AssignTo { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }
    }
}
