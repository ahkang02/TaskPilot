using Microsoft.AspNetCore.Identity;

namespace TaskPilot.Domain.Entities
{
    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole? Role { get; set; }

    }
}
