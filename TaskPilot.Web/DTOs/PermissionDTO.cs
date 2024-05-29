namespace TaskPilot.Web.DTOs
{
    public class PermissionDTO
    {
        public required Guid Id { get; set; }

        public required string Name { get; set; }

        public required DateTime Created { get; set; }

        public required DateTime Updated { get; set; }
    }
}
