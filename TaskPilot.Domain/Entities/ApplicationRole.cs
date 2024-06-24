using Microsoft.AspNetCore.Identity;

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
