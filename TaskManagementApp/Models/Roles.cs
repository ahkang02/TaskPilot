using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskManagementApp.Models
{
    public class Roles : IdentityRole
    {
        public Roles()
        {
            this.Permissions = new HashSet<Permission>();
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; }

    }
}