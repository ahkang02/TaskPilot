namespace TaskPilot.Web.DTOs
{
    public class PermissionDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}
