using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPilot.Domain.Entities
{
    public class Notifications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required string Description { get; set; }

        public required string Status { get; set; }

        public Guid? TasksId { get; set; }

        public Tasks? Tasks { get; set; }

        public required string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
