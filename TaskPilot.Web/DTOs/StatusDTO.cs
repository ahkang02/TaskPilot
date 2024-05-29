namespace TaskPilot.Web.DTOs
{
    public class StatusDTO
    {
        public required Guid Id { get; set; }

        public required string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
