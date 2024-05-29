using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TaskPilot.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
            this.Permissions = new HashSet<Permission>();
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; }

        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }

        public virtual ICollection<ApplicationRoleClaim>? RoleClaims { get; set; }

    }
}
