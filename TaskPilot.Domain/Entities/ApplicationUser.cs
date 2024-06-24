using Microsoft.AspNetCore.Identity;

namespace TaskPilot.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Notifications = new HashSet<Notifications>();
        }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public virtual ICollection<Notifications> Notifications { get; set; }

        public virtual ICollection<ApplicationUserClaim>? Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin>? Logins { get; set; }
        public virtual ICollection<ApplicationUserToken>? Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
