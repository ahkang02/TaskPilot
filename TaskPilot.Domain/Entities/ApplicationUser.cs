using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
