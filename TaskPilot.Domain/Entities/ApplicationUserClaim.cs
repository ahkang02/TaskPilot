using Microsoft.AspNetCore.Identity;

namespace TaskPilot.Domain.Entities
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}
