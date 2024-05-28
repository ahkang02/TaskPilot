namespace TaskPilot.Web.DTOs
{
    public class PriorityDTO
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
