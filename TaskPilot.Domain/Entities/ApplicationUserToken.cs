using Microsoft.AspNetCore.Identity;

namespace TaskPilot.Domain.Entities
{
    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}
