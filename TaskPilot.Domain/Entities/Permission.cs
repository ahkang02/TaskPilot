using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<ApplicationRole> Roles { get; set; }

        public Guid FeaturesId { get; set; }
        public Features Features { get; set; }
    }
}
