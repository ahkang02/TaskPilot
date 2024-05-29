namespace TaskPilot.Web.DTOs
{
    public class UserDTO
    {
        public required string Id { get; set; }

        public required string Username { get; set; }

        public required string Email { get; set; }

        public required string Name { get; set; }

        public int AccessFailedCount { get; set; }

        public required string UserRole { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? LastUpdated { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
