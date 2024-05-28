namespace TaskPilot.Web.DTOs
{
    public class StatusDTO
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
