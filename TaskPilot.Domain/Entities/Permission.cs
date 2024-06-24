using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPilot.Domain.Entities
{
    public class Permission
    {
        public Permission()
        {
            this.Roles = new HashSet<ApplicationRole>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<ApplicationRole> Roles { get; set; }

        public required Guid FeaturesId { get; set; }

        public required Features Features { get; set; }
    }
}
