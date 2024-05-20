namespace TaskPilot.Web.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public int AccessFailedCount { get; set; }

        public string UserRole { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? LastUpdated { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
