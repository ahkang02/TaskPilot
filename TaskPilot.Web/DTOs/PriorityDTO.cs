namespace TaskPilot.Web.DTOs
{
    public class PriorityDTO
    {
        public required Guid Id { get; set; }

        public required string Description { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime UpdatedAt { get; set; }
    }
}
